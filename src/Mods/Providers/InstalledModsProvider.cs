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
        private readonly ConfigEntity _config;

        public InstalledModsProvider(ConfigProvider configProvider)
        {
            _config = configProvider.Config;
        }

        /// <summary>
        /// Get list of installed mods
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="modTypeEnum">Mod type enum</param>
        public Dictionary<Guid, IMod> GetMods(IGame game, ModTypeEnum modTypeEnum)
        {
            //List<IMod> mods = [];

            string path;

            if (modTypeEnum is ModTypeEnum.Campaign)
            {
                path = game.CampaignsFolderPath;
            }
            else if (modTypeEnum is ModTypeEnum.Map)
            {
                path = game.MapsFolderPath;
            }
            else if (modTypeEnum is ModTypeEnum.Autoload)
            {
                path = game.ModsFolderPath;
            }
            else
            {
                ThrowHelper.NotImplementedException(nameof(modTypeEnum));
                return null;
            }

            var files = Directory.GetFiles(path);

            Dictionary<Guid, IMod> addedMods = [];

            foreach (var file in files)
            {
                IMod mod;

                Guid guid = Guid.NewGuid();
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

                try
                {
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
                            return [];
                        }
                    }

                    if (addedMods.TryGetValue(guid, out var addedMod))
                    {
                        if (addedMod.Version is null &&
                            mod.Version is not null)
                        {
                            //replacing with mod that have version
                            addedMods[guid] = mod;
                        }
                        else if (addedMod.Version is not null &&
                                 mod.Version is not null &&
                                 addedMod.Version < mod.Version)
                        {
                            //replacing with mod that have higher version
                            addedMods[guid] = mod;
                        }
                    }
                    else
                    {
                        addedMods.Add(guid, mod);
                    }
                }
                catch
                {
                    continue;
                }

            }

            return addedMods;
        }
    }
}
