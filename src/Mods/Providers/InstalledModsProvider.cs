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
        /// <summary>
        /// Get list of installed mods
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="modTypeEnum">Mod type enum</param>
        public List<IMod> GetMods(IGame game, ModTypeEnum modTypeEnum)
        {
            List<IMod> mods = new();

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

            foreach (var file in files)
            {
                string displayName = Path.GetFileNameWithoutExtension(file);
                string description = string.Empty;
                DukeAddonEnum dukeAddonEnum = DukeAddonEnum.Duke3D;
                WangAddonEnum wangAddonEnum = WangAddonEnum.Wang;
                BloodAddonEnum bloodAddonEnum = BloodAddonEnum.Blood;
                List<PortEnum>? supportedPorts = null;
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
                                displayName = manifest.Name;
                                supportedPorts = manifest.SupportedPorts;
                                description = manifest.Description;
                                startupFile = manifest.StartupFile;
                                dukeAddonEnum = manifest.DukeAddon is not null ? (DukeAddonEnum)manifest.DukeAddon : DukeAddonEnum.Duke3D;
                                wangAddonEnum = manifest.WangAddon is not null ? (WangAddonEnum)manifest.WangAddon : WangAddonEnum.Wang;
                                bloodAddonEnum = manifest.BloodAddon is not null ? (BloodAddonEnum)manifest.BloodAddon : BloodAddonEnum.Blood;
                                version = manifest.Version;
                                url = manifest.Url;
                                author = manifest.Author;
                                image = ImageHelper.GetCoverFromArchive(archive);
                            }
                        }
                    }

                    if (modTypeEnum is ModTypeEnum.Autoload)
                    {
                        mods.Add(new AutoloadMod()
                        {
                            ModType = ModTypeEnum.Autoload,
                            DisplayName = displayName,
                            Image = image,
                            SupportedPorts = supportedPorts,
                            Description = description,
                            Version = version,
                            Author = author,
                            Url = url,
                            IsEnabled = true,
                            IsOfficial = false,
                            PathToFile = file,
                            StartupFile = null
                        });
                    }
                    else
                    {
                        if (game.GameEnum is GameEnum.Duke3D)
                        {
                            mods.Add(new DukeCampaign()
                            {
                                ModType = modTypeEnum,
                                DisplayName = displayName,
                                Image = image,
                                SupportedPorts = supportedPorts,
                                Description = description,
                                StartupFile = startupFile,
                                AddonEnum = dukeAddonEnum,
                                Version = version,
                                Author = author,
                                Url = url,
                                IsOfficial = false,
                                PathToFile = file
                            });
                        }
                        else if (game.GameEnum is GameEnum.Wang)
                        {
                            mods.Add(new WangCampaign()
                            {
                                ModType = modTypeEnum,
                                DisplayName = displayName,
                                Image = image,
                                SupportedPorts = supportedPorts,
                                Description = description,
                                AddonEnum = wangAddonEnum,
                                Version = version,
                                Author = author,
                                Url = url,
                                IsOfficial = false,
                                PathToFile = file,
                                StartupFile = null
                            });
                        }
                        else if (game.GameEnum is GameEnum.Blood)
                        {
                            mods.Add(new BloodCampaign()
                            {
                                ModType = modTypeEnum,
                                DisplayName = displayName,
                                Image = image,
                                SupportedPorts = supportedPorts,
                                Description = description,
                                AddonEnum = bloodAddonEnum,
                                Version = version,
                                Author = author,
                                Url = url,
                                StartupFile = startupFile!,
                                IsOfficial = false,
                                PathToFile = file
                            });
                        }
                    }
                }
                catch
                {
                    continue;
                }

            }

            return mods;
        }
    }
}
