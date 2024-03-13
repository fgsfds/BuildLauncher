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
    public sealed class InstalledModsProvider
    {
        private readonly Dictionary<GameEnum, Dictionary<ModTypeEnum, Dictionary<Guid, IMod>>>? _cache = [];
        private readonly SemaphoreSlim _semaphore = new(1);
        
        private readonly ConfigEntity _config;

        public delegate void ModInstalled(IGame game, ModTypeEnum modType);
        public event ModInstalled NotifyModDeleted;

        public InstalledModsProvider(ConfigProvider configProvider)
        {
            _config = configProvider.Config;
        }


        /// <summary>
        /// Update cached list of installed mods
        /// </summary>
        public async Task UpdateCachedListAsync(IGame game)
        {
            _semaphore.Wait();

            if (_cache is null || !_cache.TryGetValue(game.GameEnum, out _))
            {
                await UpdateCacheAsync(game);
            }

            _semaphore.Release();

            _cache.ThrowIfNull();
        }

        /// <summary>
        /// Delete mod from cache and disk
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="mod">Mod</param>
        public void DeleteMod(IGame game, IMod mod)
        {
            _cache.ThrowIfNull();

            File.Delete(mod.PathToFile!);

            _cache[game.GameEnum][mod.ModType].Remove(mod.Guid);

            NotifyModDeleted?.Invoke(game, mod.ModType);
        }

        /// <summary>
        /// Add mod to cache
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="modTypeEnum">Mod type</param>
        /// <param name="pathToFile">Path to mod file</param>
        public void AddMod(IGame game, ModTypeEnum modTypeEnum, string pathToFile)
        {
            _cache.ThrowIfNull();

            var mod = GetMod(game, modTypeEnum, pathToFile);

            if (!_cache[game.GameEnum].TryGetValue(mod.ModType, out _))
            {
                _cache[game.GameEnum].Add(mod.ModType, []);
            }

            var dict = _cache[game.GameEnum][mod.ModType];

            if (dict.TryGetValue(mod.Guid, out _))
            {
                dict[mod.Guid] = mod;
            }
            else
            {
                dict.Add(mod.Guid, mod);
            }
        }

        /// <summary>
        /// Get installed mods
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="modTypeEnum">Mod type</param>
        public Dictionary<Guid, IMod> GetMods(IGame game, ModTypeEnum modTypeEnum)
        {
            _cache.ThrowIfNull();

            try
            {
                return _cache[game.GameEnum][modTypeEnum];
            }
            catch
            {
                return [];
            }
        }

        /// <summary>
        /// Get list of installed mods
        /// </summary>
        /// <param name="game">Game</param>
        private async Task UpdateCacheAsync(IGame game)
        {
            _cache.ThrowIfNull();

            _cache.Add(game.GameEnum, []);

            await Task.Run(() =>
            {
                string[] files;
                ModTypeEnum modTypeEnum;

                files = Directory.GetFiles(game.CampaignsFolderPath);
                modTypeEnum = ModTypeEnum.Campaign;
                var camps = GetModsList(game, modTypeEnum, files);
                _cache[game.GameEnum].Add(modTypeEnum, camps);

                files = Directory.GetFiles(game.MapsFolderPath);
                modTypeEnum = ModTypeEnum.Map;
                var maps = GetModsList(game, modTypeEnum, files);
                _cache[game.GameEnum].Add(modTypeEnum, maps);

                files = Directory.GetFiles(game.ModsFolderPath);
                modTypeEnum = ModTypeEnum.Autoload;
                var mods = GetModsList(game, modTypeEnum, files);
                _cache[game.GameEnum].Add(modTypeEnum, mods);
            });
        }

        /// <summary>
        /// Get mods dictionary
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="modTypeEnum">Mod type</param>
        /// <param name="files">Paths to mod files</param>
        private Dictionary<Guid, IMod> GetModsList(IGame game, ModTypeEnum modTypeEnum, IEnumerable<string> files)
        {
            Dictionary<Guid, IMod> addedMods = [];

            foreach (var file in files)
            {
                try
                {
                    var mod = GetMod(game, modTypeEnum, file);

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
        /// Get mod from file
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="modTypeEnum">Mod type</param>
        /// <param name="file">Path to mod file</param>
        /// <returns></returns>
        private IMod GetMod(IGame game, ModTypeEnum modTypeEnum, string file)
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
                    StartupFile = null
                };
            }
            else
            {
                if (game.GameEnum is GameEnum.Duke3D)
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
                        PathToFile = file
                    };
                }
                else if (game.GameEnum is GameEnum.Wang)
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
                        StartupFile = null
                    };
                }
                else if (game.GameEnum is GameEnum.Blood)
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
                        PathToFile = file
                    };
                }
                else if (game.GameEnum is GameEnum.Fury)
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
                        PathToFile = file
                    };
                }
                else if (game.GameEnum is GameEnum.Slave)
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
                        PathToFile = file
                    };
                }
                else if (game.GameEnum is GameEnum.Redneck)
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
                        PathToFile = file
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
