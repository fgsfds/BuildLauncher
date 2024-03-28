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
        private readonly Dictionary<ModTypeEnum, Dictionary<string, IAddon>> _cache;
        private readonly SemaphoreSlim _semaphore = new(1);

        public event ModChanged ModDeletedEvent;

        [Obsolete($"Don't create directly. Use {nameof(InstalledModsProvider)}.")]
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

                    files = Directory.GetFiles(_game.CampaignsFolderPath).Where(static x => x.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase) || x.EndsWith(".grp", StringComparison.InvariantCultureIgnoreCase));
                    var camps = GetModsFromFiles(ModTypeEnum.TC, files);
                    _cache.Add(ModTypeEnum.TC, camps);

                    files = Directory.GetFiles(_game.MapsFolderPath).Where(static x => x.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase) || x.EndsWith(".map", StringComparison.InvariantCultureIgnoreCase));
                    var maps = GetModsFromFiles(ModTypeEnum.Map, files);
                    _cache.Add(ModTypeEnum.Map, maps);

                    files = Directory.GetFiles(_game.ModsFolderPath, "*.zip");
                    var mods = GetModsFromFiles(ModTypeEnum.Mod, files);
                    _cache.Add(ModTypeEnum.Mod, mods);
                });
            }

            _semaphore.Release();

            _cache.ThrowIfNull();
        }

        /// <inheritdoc/>
        public void DeleteMod(IAddon mod)
        {
            _cache.ThrowIfNull();

            File.Delete(mod.PathToFile!);

            _cache[mod.ModType].Remove(mod.Id);

            ModDeletedEvent?.Invoke(_game, mod.ModType);
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

            if (dict.TryGetValue(mod.Id, out _))
            {
                dict[mod.Id] = mod;
            }
            else
            {
                dict.Add(mod.Id, mod);
            }
        }

        /// <inheritdoc/>
        public Dictionary<string, IAddon> GetInstalledMods(ModTypeEnum modTypeEnum)
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
        private Dictionary<string, IAddon> GetModsFromFiles(ModTypeEnum modTypeEnum, IEnumerable<string> files)
        {
            Dictionary<string, IAddon> addedMods = [];

            foreach (var file in files)
            {
                try
                {
                    var mod = GetMod(modTypeEnum, file);

                    if (mod is null)
                    {
                        continue;
                    }

                    if (addedMods.TryGetValue(mod.Id, out var addedMod))
                    {
                        if (addedMod.Version is null &&
                            mod.Version is not null)
                        {
                            //replacing with mod that have version
                            addedMods[mod.Id] = mod;
                        }
                        else if (addedMod.Version is not null &&
                                 mod.Version is not null &&
                                 string.CompareOrdinal(addedMod.Version, mod.Version) == -1)
                        {
                            //replacing with mod that have higher version
                            addedMods[mod.Id] = mod;
                        }
                    }
                    else
                    {
                        addedMods.Add(mod.Id, mod);
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
        private IAddon? GetMod(ModTypeEnum modTypeEnum, string file)
        {
            IAddon mod;

            var id = Guid.NewGuid().ToString();
            string title = Path.GetFileNameWithoutExtension(file);
            string description = string.Empty;
            List<PortEnum>? supportedPorts = null;
            List<string>? supportedAddons = null;
            string? startupFile = null;
            string? version = null;
            string? author = null;
            Stream? image = null;
            bool isLoose = false;
            string? defFileContents = null;

            if (ArchiveFactory.IsArchive(file, out var _))
            {
                using var archive = ArchiveFactory.Open(file);
                var entry = archive.Entries.FirstOrDefault(static x => x.Key.Equals("addon.json"));

                if (entry is not null)
                {
                    var manifest = JsonSerializer.Deserialize(
                        entry.OpenEntryStream(),
                        AddonManifestContext.Default.AddonManifest
                        );

                    if (manifest is not null)
                    {
                        id = manifest.Id;
                        title = manifest.Title;
                        addon = manifest.Addon;
                        supportedPorts = manifest.SupportedPorts;
                        supportedAddons = manifest.SupportedAddons;
                        description = manifest.Description;
                        startupFile = manifest.StartupFile;
                        version = manifest.Version;
                        author = manifest.Author;
                        image = ImageHelper.GetCoverFromArchive(archive);
                    }
                }
                else
                {
                    if (_game.GameEnum is GameEnum.Blood && modTypeEnum is ModTypeEnum.TC)
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
                    modTypeEnum is ModTypeEnum.TC)
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
            else if (file.EndsWith(".grp", StringComparison.InvariantCultureIgnoreCase))
            {
                //"real" grps are not supported
                return null;
            }

            if (modTypeEnum is ModTypeEnum.Mod)
            {
                var isEnabled = !_config.DisabledAutoloadMods.Contains(id);

                mod = new AutoloadMod()
                {
                    Id = id,
                    ModType = ModTypeEnum.Mod,
                    Title = title,
                    Image = image,
                    SupportedPorts = supportedPorts,
                    SupportedAddons = supportedAddons,
                    Description = description,
                    Version = version,
                    Author = author,
                    IsEnabled = isEnabled,
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
                        Id = id,
                        ModType = modTypeEnum,
                        Title = title,
                        Image = image,
                        SupportedPorts = supportedPorts,
                        Description = description,
                        StartupFile = startupFile,
                        AddonEnum = addon is null ? DukeAddonEnum.Duke3D : Enum.Parse<DukeAddonEnum>(addon),
                        Version = version,
                        Author = author,
                        PathToFile = file,
                        IsLoose = isLoose,
                        DefFileContents = defFileContents
                    };
                }
                else if (_game.GameEnum is GameEnum.Wang)
                {
                    mod = new WangCampaign()
                    {
                        Id = id,
                        ModType = modTypeEnum,
                        Title = title,
                        Image = image,
                        SupportedPorts = supportedPorts,
                        Description = description,
                        AddonEnum = addon is null ? WangAddonEnum.Wang : Enum.Parse<WangAddonEnum>(addon),
                        Version = version,
                        Author = author,
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
                        Id = id,
                        ModType = modTypeEnum,
                        Title = title,
                        Image = image,
                        SupportedPorts = supportedPorts,
                        Description = description,
                        AddonEnum = addon is null ? BloodAddonEnum.Blood : Enum.Parse<BloodAddonEnum>(addon),
                        Version = version,
                        Author = author,
                        StartupFile = startupFile!,
                        PathToFile = file,
                        IsLoose = isLoose,
                        DefFileContents = defFileContents
                    };
                }
                else if (_game.GameEnum is GameEnum.Fury)
                {
                    mod = new FuryCampaign()
                    {
                        Id = id,
                        ModType = modTypeEnum,
                        Title = title,
                        Image = image,
                        SupportedPorts = supportedPorts,
                        Description = description,
                        Version = version,
                        Author = author,
                        StartupFile = startupFile!,
                        PathToFile = file,
                        IsLoose = isLoose,
                        DefFileContents = defFileContents
                    };
                }
                else if (_game.GameEnum is GameEnum.Slave)
                {
                    mod = new SlaveCampaign()
                    {
                        Id = id,
                        ModType = modTypeEnum,
                        Title = title,
                        Image = image,
                        SupportedPorts = supportedPorts,
                        Description = description,
                        Version = version,
                        Author = author,
                        StartupFile = startupFile!,
                        PathToFile = file,
                        IsLoose = isLoose,
                        DefFileContents = defFileContents
                    };
                }
                else if (_game.GameEnum is GameEnum.Redneck)
                {
                    mod = new RedneckCampaign()
                    {
                        Id = id,
                        ModType = modTypeEnum,
                        Title = title,
                        Image = image,
                        SupportedPorts = supportedPorts,
                        Description = description,
                        AddonEnum = addon is null ? RedneckAddonEnum.Redneck : Enum.Parse<RedneckAddonEnum>(addon),
                        Version = version,
                        Author = author,
                        StartupFile = startupFile!,
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
