using Common.Config;
using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Mods.Mods;
using Mods.Serializable;
using SharpCompress.Archives;
using System.Text.Json;

namespace Mods.Providers
{
    /// <summary>
    /// Class that provides lists of installed mods
    /// </summary>
    public sealed class InstalledModsProvider : IInstalledModsProvider
    {
        private readonly IGame _game;
        private readonly ConfigEntity _config;
        private readonly Dictionary<ModTypeEnum, Dictionary<Guid, IMod>> _cache;
        private readonly SemaphoreSlim _semaphore = new(1);

        public event ModInstalled NotifyModDeleted;

        public InstalledModsProvider(
            IGame game,
            ConfigEntity config
            )
        {
            _game = game;
            _config = config;
            _cache = [];
        }


        /// <inheritdoc/>
        public async Task CreateCache(bool createNew)
        {
            _semaphore.Wait();

            if (createNew)
            {
                _cache.Clear();
            }

            if (_cache.Count == 0)
            {
                await Task.Run(() =>
                {
                    IEnumerable<string> files;

                    files = Directory.GetFiles(_game.CampaignsFolderPath, "*.zip");
                    var camps = GetModsFromFiles(ModTypeEnum.Campaign, files);
                    _cache.Add(ModTypeEnum.Campaign, camps);

                    files = Directory.GetFiles(_game.MapsFolderPath, "*.zip");
                    files = files.Union(Directory.GetFiles(_game.MapsFolderPath, "*.map"));
                    var maps = GetModsFromFiles(ModTypeEnum.Map, files);
                    _cache.Add(ModTypeEnum.Map, maps);

                    files = Directory.GetFiles(_game.ModsFolderPath, "*.zip");
                    var mods = GetModsFromFiles(ModTypeEnum.Autoload, files);
                    _cache.Add(ModTypeEnum.Autoload, mods);
                });
            }

            _semaphore.Release();

            _cache.ThrowIfNull();
        }

        /// <inheritdoc/>
        public void DeleteMod(IMod mod)
        {
            _cache.ThrowIfNull();

            File.Delete(mod.PathToFile!);

            _cache[mod.ModType].Remove(mod.Guid);

            NotifyModDeleted?.Invoke(_game, mod.ModType);
        }

        /// <inheritdoc/>
        public void AddMod(ModTypeEnum modTypeEnum, string pathToFile)
        {
            _cache.ThrowIfNull();

            var mod = GetMod(modTypeEnum, pathToFile);

            if (!_cache.TryGetValue(mod.ModType, out _))
            {
                _cache.Add(mod.ModType, []);
            }

            var dict = _cache[mod.ModType];

            if (dict.TryGetValue(mod.Guid, out _))
            {
                dict[mod.Guid] = mod;
            }
            else
            {
                dict.Add(mod.Guid, mod);
            }
        }

        /// <inheritdoc/>
        public Dictionary<Guid, IMod> GetInstalledMods(ModTypeEnum modTypeEnum)
        {
            _cache.ThrowIfNull();

            try
            {
                return _cache[modTypeEnum];
            }
            catch
            {
                return [];
            }
        }

        /// <summary>
        /// Get mods from list of files
        /// </summary>
        /// <param name="modTypeEnum">Mod type</param>
        /// <param name="files">Paths to mod files</param>
        private Dictionary<Guid, IMod> GetModsFromFiles(ModTypeEnum modTypeEnum, IEnumerable<string> files)
        {
            Dictionary<Guid, IMod> addedMods = [];

            foreach (var file in files)
            {
                try
                {
                    var mod = GetMod(modTypeEnum, file);

                    if (addedMods.TryGetValue(mod.Guid, out var addedMod))
                    {
                        if (addedMod.Version is null &&
                            mod.Version is not null)
                        {
                            //replacing with mod that have version
                            addedMods[mod.Guid] = mod;
                        }
                        else if (addedMod.Version is not null &&
                                 mod.Version is not null &&
                                 addedMod.Version < mod.Version)
                        {
                            //replacing with mod that have higher version
                            addedMods[mod.Guid] = mod;
                        }
                    }
                    else
                    {
                        addedMods.Add(mod.Guid, mod);
                    }
                }
                catch
                {
                    continue;
                }

            }

            return addedMods;
        }

        /// <summary>
        /// Get mod from a file
        /// </summary>
        /// <param name="modTypeEnum">Mod type</param>
        /// <param name="file">Path to mod file</param>
        private IMod GetMod(ModTypeEnum modTypeEnum, string file)
        {
            IMod mod;

            var guid = Guid.NewGuid();
            string displayName = Path.GetFileNameWithoutExtension(file);
            string description = string.Empty;
            string? addon = null;
            List<PortEnum>? supportedPorts = null;
            List<string>? supportedAddons = null;
            string? startupFile = null;
            float? version = null;
            string? url = null;
            string? author = null;
            Stream? image = null;
            bool isLoose = false;
            string? defFileContents = null;

            if (ArchiveFactory.IsArchive(file, out var _))
            {
                using var archive = ArchiveFactory.Open(file);
                var entry = archive.Entries.FirstOrDefault(static x => x.Key.Equals("manifest.json"));

                if (entry is not null)
                {
                    var manifest = JsonSerializer.Deserialize(
                        entry.OpenEntryStream(),
                        ModManifestContext.Default.ModManifest
                        );

                    if (manifest is not null)
                    {
                        if (manifest.ModType is not ModTypeEnum.Autoload &&
                            manifest.SupportedAddons is not null)
                        {
                            ThrowHelper.NotImplementedException("SupportedAddons property is only supported by Autoload mods");
                        }

                        guid = manifest.Guid;
                        displayName = manifest.Name;
                        addon = manifest.Addon;
                        supportedPorts = manifest.SupportedPorts;
                        supportedAddons = manifest.SupportedAddons;
                        description = manifest.Description;
                        startupFile = manifest.StartupFile;
                        version = manifest.Version;
                        url = manifest.Url;
                        author = manifest.Author;
                        image = ImageHelper.GetCoverFromArchive(archive);
                    }
                }
                else
                {
                    if (_game.GameEnum is GameEnum.Blood && modTypeEnum is ModTypeEnum.Campaign)
                    {
                        var ini = archive.Entries.FirstOrDefault(static x => x.Key.EndsWith(".ini", StringComparison.InvariantCultureIgnoreCase));

                        if (ini is not null)
                        {
                            startupFile = ini.Key;
                        }
                    }
                }

                var defFile = archive.Entries.FirstOrDefault(x => x.Key.Equals(_game.DefFile));

                if (defFile is not null &&
                    modTypeEnum is ModTypeEnum.Campaign)
                {
                    using var stream = defFile.OpenEntryStream();
                    using StreamReader reader = new(stream);

                    defFileContents = reader.ReadToEnd();
                }
            }
            else if (file.EndsWith(".map", StringComparison.InvariantCultureIgnoreCase))
            {
                isLoose = true;
                startupFile = Path.GetFileName(file);
            }

            if (modTypeEnum is ModTypeEnum.Autoload)
            {
                var isEnabled = !_config.DisabledAutoloadMods.Contains(guid);

                mod = new AutoloadMod()
                {
                    Guid = guid,
                    ModType = ModTypeEnum.Autoload,
                    DisplayName = displayName,
                    Image = image,
                    SupportedPorts = supportedPorts,
                    SupportedAddons = supportedAddons,
                    Description = description,
                    Version = version,
                    Author = author,
                    Url = url,
                    IsEnabled = isEnabled,
                    IsOfficial = false,
                    PathToFile = file,
                    StartupFile = null,
                    IsLoose = isLoose
                };
            }
            else
            {
                if (_game.GameEnum is GameEnum.Duke3D)
                {
                    mod = new DukeCampaign()
                    {
                        Guid = guid,
                        ModType = modTypeEnum,
                        DisplayName = displayName,
                        Image = image,
                        SupportedPorts = supportedPorts,
                        Description = description,
                        StartupFile = startupFile,
                        AddonEnum = addon is null ? DukeAddonEnum.Duke3D : Enum.Parse<DukeAddonEnum>(addon),
                        Version = version,
                        Author = author,
                        Url = url,
                        IsOfficial = false,
                        PathToFile = file,
                        IsLoose = isLoose,
                        DefFileContents = defFileContents
                    };
                }
                else if (_game.GameEnum is GameEnum.Wang)
                {
                    mod = new WangCampaign()
                    {
                        Guid = guid,
                        ModType = modTypeEnum,
                        DisplayName = displayName,
                        Image = image,
                        SupportedPorts = supportedPorts,
                        Description = description,
                        AddonEnum = addon is null ? WangAddonEnum.Wang : Enum.Parse<WangAddonEnum>(addon),
                        Version = version,
                        Author = author,
                        Url = url,
                        IsOfficial = false,
                        PathToFile = file,
                        StartupFile = null,
                        IsLoose = isLoose,
                        DefFileContents = defFileContents
                    };
                }
                else if (_game.GameEnum is GameEnum.Blood)
                {
                    mod = new BloodCampaign()
                    {
                        Guid = guid,
                        ModType = modTypeEnum,
                        DisplayName = displayName,
                        Image = image,
                        SupportedPorts = supportedPorts,
                        Description = description,
                        AddonEnum = addon is null ? BloodAddonEnum.Blood : Enum.Parse<BloodAddonEnum>(addon),
                        Version = version,
                        Author = author,
                        Url = url,
                        StartupFile = startupFile!,
                        IsOfficial = false,
                        PathToFile = file,
                        IsLoose = isLoose,
                        DefFileContents = defFileContents
                    };
                }
                else if (_game.GameEnum is GameEnum.Fury)
                {
                    mod = new FuryCampaign()
                    {
                        Guid = guid,
                        ModType = modTypeEnum,
                        DisplayName = displayName,
                        Image = image,
                        SupportedPorts = supportedPorts,
                        Description = description,
                        Version = version,
                        Author = author,
                        Url = url,
                        StartupFile = startupFile!,
                        IsOfficial = false,
                        PathToFile = file,
                        IsLoose = isLoose,
                        DefFileContents = defFileContents
                    };
                }
                else if (_game.GameEnum is GameEnum.Slave)
                {
                    mod = new SlaveCampaign()
                    {
                        Guid = guid,
                        ModType = modTypeEnum,
                        DisplayName = displayName,
                        Image = image,
                        SupportedPorts = supportedPorts,
                        Description = description,
                        Version = version,
                        Author = author,
                        Url = url,
                        StartupFile = startupFile!,
                        IsOfficial = false,
                        PathToFile = file,
                        IsLoose = isLoose,
                        DefFileContents = defFileContents
                    };
                }
                else if (_game.GameEnum is GameEnum.Redneck)
                {
                    mod = new RedneckCampaign()
                    {
                        Guid = guid,
                        ModType = modTypeEnum,
                        DisplayName = displayName,
                        Image = image,
                        SupportedPorts = supportedPorts,
                        Description = description,
                        AddonEnum = addon is null ? RedneckAddonEnum.Redneck : Enum.Parse<RedneckAddonEnum>(addon),
                        Version = version,
                        Author = author,
                        Url = url,
                        StartupFile = startupFile!,
                        IsOfficial = false,
                        PathToFile = file,
                        IsLoose = isLoose,
                        DefFileContents = defFileContents
                    };
                }
                else
                {
                    ThrowHelper.NotImplementedException();
                    return null;
                }
            }

            return mod;
        }
    }
}
