﻿using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Mods.Mods;
using Mods.Providers;
using System.IO.Compression;
using System.Text;

namespace Games.Games
{
    /// <summary>
    /// Base class that encapsulates logic for working with games and their mods
    /// </summary>
    public abstract class BaseGame : IGame
    {
        /// <inheritdoc/>
        public string? GameInstallFolder { get; set; }

        /// <inheritdoc/>
        public bool IsBaseGameInstalled => IsInstalled(RequiredFiles);

        /// <inheritdoc/>
        public string CampaignsFolderPath => Path.Combine(CommonProperties.DataFolderPath, ShortName, "Campaigns");

        /// <inheritdoc/>
        public string MapsFolderPath => Path.Combine(CommonProperties.DataFolderPath, ShortName, "Maps");

        /// <inheritdoc/>
        public string ModsFolderPath => Path.Combine(CommonProperties.DataFolderPath, ShortName, "Mods");

        /// <inheritdoc/>
        public string SpecialFolderPath => Path.Combine(CommonProperties.DataFolderPath, ShortName, "Special");


        /// <inheritdoc/>
        public abstract GameEnum GameEnum { get; }

        /// <inheritdoc/>
        public abstract string FullName { get; }

        /// <inheritdoc/>
        public abstract string ShortName { get; }

        /// <inheritdoc/>
        public abstract string DefFile { get; }

        /// <inheritdoc/>
        public abstract List<string> RequiredFiles { get; }

        public IInstalledModsProvider InstalledModsProvider { get; init; }

        public IDownloadableModsProvider DownloadableModsProvider { get; init; }


        public BaseGame(
            InstalledModsProviderFactory installedModsProviderFactory,
            DownloadableModsProviderFactory downloadableModsProviderFactory
            )
        {
            InstalledModsProvider = installedModsProviderFactory.GetSingleton(this);
            DownloadableModsProvider = downloadableModsProviderFactory.GetSingleton(this);

            if (!Directory.Exists(CampaignsFolderPath))
            {
                Directory.CreateDirectory(CampaignsFolderPath);
            }

            if (!Directory.Exists(MapsFolderPath))
            {
                Directory.CreateDirectory(MapsFolderPath);
            }

            if (!Directory.Exists(ModsFolderPath))
            {
                Directory.CreateDirectory(ModsFolderPath);
            }

            if (!Directory.Exists(SpecialFolderPath))
            {
                Directory.CreateDirectory(SpecialFolderPath);
            }
        }


        /// <inheritdoc/>
        public virtual Dictionary<Guid, IMod> GetCampaigns()
        {
            Dictionary<Guid, IMod> originalCampaigns = GetOriginalCampaigns();

            var customCampaigns = InstalledModsProvider.GetInstalledMods(ModTypeEnum.Campaign);

            foreach (var customCamp in customCampaigns)
            {
                if (originalCampaigns.TryGetValue(customCamp.Key, out var originalCamp))
                {
                    if (originalCamp.Version is null &&
                        customCamp.Value.Version is not null)
                    {
                        //replacing with mod that have version
                        originalCampaigns[customCamp.Key] = customCamp.Value;
                    }
                    else if (customCamp.Value.Version is not null &&
                             originalCamp.Version is not null &&
                             customCamp.Value.Version > originalCamp.Version)
                    {
                        //replacing with mod that have higher version
                        originalCampaigns[customCamp.Key] = customCamp.Value;
                    }
                }
                else
                {
                    originalCampaigns.Add(customCamp.Key, customCamp.Value);
                }
            }

            return originalCampaigns;
        }


        /// <inheritdoc/>
        public virtual Dictionary<Guid, IMod> GetSingleMaps()
        {
            var maps = InstalledModsProvider.GetInstalledMods(ModTypeEnum.Map);

            return maps;
        }


        /// <inheritdoc/>
        public virtual Dictionary<Guid, IMod> GetAutoloadMods(bool enabledOnly)
        {
            var mods = InstalledModsProvider.GetInstalledMods(ModTypeEnum.Autoload);

            if (enabledOnly)
            {
                Dictionary<Guid, IMod> filtered = [];

                foreach (var mod in mods)
                {
                    if (((AutoloadMod)mod.Value).IsEnabled)
                    {
                        filtered.Add(mod.Key, mod.Value);
                    }
                }

                return filtered;
            }

            return mods;
        }


        /// <inheritdoc/>
        public void CreateCombinedMod(string? additionalDef = null)
        {
            Cleanup();

            if (!Directory.Exists(ModsFolderPath))
            {
                return;
            }

            var combinedFolderPath = Path.Combine(SpecialFolderPath, Consts.CombinedModFolder);

            if (Directory.Exists(combinedFolderPath))
            {
                Directory.Delete(combinedFolderPath, true);
            }

            StringBuilder newDef = new();

            var files = GetAutoloadMods(true);

            if (files.Count == 0)
            {
                return;
            }

            foreach (var file in files)
            {
                file.Value.ThrowIfNotType<AutoloadMod>(out var autoloadMod);
                autoloadMod.PathToFile.ThrowIfNull();

                if (!autoloadMod.IsEnabled)
                {
                    continue;
                }

                using var zip = ZipFile.OpenRead(autoloadMod.PathToFile);

                var ini = zip.Entries.FirstOrDefault(x => x.Name.Equals(DefFile));

                if (ini is null)
                {
                    continue;
                }

                using var stream = ini.Open();
                using var streamReader = new StreamReader(stream);

                newDef.Append(streamReader.ReadToEnd());
                newDef.Append(Environment.NewLine);
            }

            if (additionalDef is not  null)
            {
                newDef.Append(additionalDef);
            }

            using var memStream = new MemoryStream(Encoding.UTF8.GetBytes(newDef.ToString()));

            if (!Directory.Exists(combinedFolderPath))
            {
                Directory.CreateDirectory(combinedFolderPath);
            }

            File.WriteAllText(Path.Combine(combinedFolderPath, Consts.CombinedDef), newDef.ToString());
        }

        /// <summary>
        /// Removing old versions of combined mod
        /// </summary>
        [Obsolete("Remove later")]
        private void Cleanup()
        {
            var combFile1 = Path.Combine(SpecialFolderPath, "combined.zip");
            var combFile2 = Path.Combine(SpecialFolderPath, "z_combined.zip");

            if (File.Exists(combFile1))
            {
                File.Delete(combFile1);
            }
            if (File.Exists(combFile2))
            {
                File.Delete(combFile2);
            }
        }


        /// <summary>
        /// Get list of original campaigns
        /// </summary>
        /// <returns></returns>
        protected abstract Dictionary<Guid, IMod> GetOriginalCampaigns();


        /// <summary>
        /// Do provided files exist in the game install folder
        /// </summary>
        /// <param name="files">List of required files</param>
        protected bool IsInstalled(List<string> files, string? path = null)
        {
            var gamePath = path is null ? GameInstallFolder : path;

            if (gamePath is null)
            {
                return false;
            }

            foreach (var file in files)
            {
                if (!File.Exists(Path.Combine(gamePath, file)))
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Does the file exists in the game install folder
        /// </summary>
        /// <param name="file">File</param>
        protected bool IsInstalled(string file)
        {
            if (GameInstallFolder is null)
            {
                return false;
            }

            if (!File.Exists(Path.Combine(GameInstallFolder, file)))
            {
                return false;
            }

            return true;
        }
    }
}
