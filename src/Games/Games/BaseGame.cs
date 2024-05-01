using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Common.Providers;
using Mods.Addons;
using Mods.Providers;

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
        public IInstalledAddonsProvider InstalledAddonsProvider { get; init; }

        /// <inheritdoc/>
        public IDownloadableAddonsProvider DownloadableAddonsProvider { get; init; }

        protected readonly PlaytimeProvider _playtimeProvider;


        /// <inheritdoc/>
        public abstract GameEnum GameEnum { get; }

        /// <inheritdoc/>
        public abstract string FullName { get; }

        /// <inheritdoc/>
        public abstract string ShortName { get; }

        /// <inheritdoc/>
        public abstract List<string> RequiredFiles { get; }


        public BaseGame(
            InstalledAddonsProviderFactory installedModsProviderFactory,
            DownloadableAddonsProviderFactory downloadableModsProviderFactory,
            PlaytimeProvider playtimeProvider
            )
        {
            InstalledAddonsProvider = installedModsProviderFactory.GetSingleton(this);
            DownloadableAddonsProvider = downloadableModsProviderFactory.GetSingleton(this);
            _playtimeProvider = playtimeProvider;

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

            Cleanup();
        }


        /// <inheritdoc/>
        public virtual Dictionary<string, IAddon> GetCampaigns()
        {
            var originalCampaigns = GetOriginalCampaigns();

            var customCampaigns = InstalledAddonsProvider.GetInstalledAddon(AddonTypeEnum.TC);

            foreach (var customCamp in customCampaigns)
            {
                if (originalCampaigns.TryGetValue(customCamp.Key, out var _))
                {
                    //replacing original campaign with the downloaded one
                    originalCampaigns[customCamp.Key] = customCamp.Value;
                }
                else
                {
                    originalCampaigns.Add(customCamp.Key, customCamp.Value);
                }
            }

            return originalCampaigns;
        }


        /// <inheritdoc/>
        public virtual Dictionary<string, IAddon> GetSingleMaps()
        {
            var maps = InstalledAddonsProvider.GetInstalledAddon(AddonTypeEnum.Map);

            return maps;
        }


        /// <inheritdoc/>
        public virtual Dictionary<string, IAddon> GetAutoloadMods(bool enabledOnly)
        {
            var mods = InstalledAddonsProvider.GetInstalledAddon(AddonTypeEnum.Mod);

            if (enabledOnly)
            {
                Dictionary<string, IAddon> enabled = new(StringComparer.OrdinalIgnoreCase);

                foreach (var mod in mods)
                {
                    if (mod.Value is not AutoloadMod aMod)
                    {
                        ThrowHelper.ArgumentException(nameof(mod));
                        return null;
                    }

                    if (aMod.IsEnabled)
                    {
                        enabled.Add(mod.Key, aMod);
                    }
                }

                return enabled;
            }

            return mods;
        }

        /// <summary>
        /// Removing old versions of combined mod
        /// </summary>
        [Obsolete("Remove later")]
        private void Cleanup()
        {
            if (Directory.Exists(SpecialFolderPath))
            {
                Directory.Delete(SpecialFolderPath, true);
            }
        }


        /// <summary>
        /// Get list of original campaigns
        /// </summary>
        /// <returns></returns>
        protected abstract Dictionary<string, IAddon> GetOriginalCampaigns();


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
        /// Does the file exist in the game install folder
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
