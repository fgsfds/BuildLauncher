using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Mods.Mods;
using Mods.Providers;
using System.Collections.Immutable;
using System.IO.Compression;
using System.Text;

namespace Games.Games
{
    /// <summary>
    /// Base class that encapsulates logic for working with games and their mods
    /// </summary>
    public abstract class BaseGame : IGame
    {
        protected readonly InstalledModsProvider _installedModsProvider;

        /// <inheritdoc/>
        public string GameInstallFolder { get; set; }

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
        public abstract string MainFile { get; }

        /// <inheritdoc/>
        public abstract string FullName { get; }

        /// <inheritdoc/>
        public abstract string ShortName { get; }

        /// <inheritdoc/>
        public abstract string DefFile { get; }

        /// <inheritdoc/>
        public abstract List<string> RequiredFiles { get; }


        public BaseGame(InstalledModsProvider modsProvider)
        {
            _installedModsProvider = modsProvider;

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

            CreateCombinedMod();
        }


        /// <inheritdoc/>
        public virtual ImmutableList<IMod> GetCampaigns()
        {
            var campaigns = GetOriginalCampaigns();

            var cusomCampaigns = _installedModsProvider.GetMods(this, ModTypeEnum.Campaign);

            return [.. campaigns, .. cusomCampaigns];
        }


        /// <inheritdoc/>
        public virtual ImmutableList<IMod> GetSingleMaps()
        {
            var maps = _installedModsProvider.GetMods(this, ModTypeEnum.Map);

            return [.. maps];
        }


        /// <inheritdoc/>
        public virtual ImmutableList<IMod> GetAutoloadMods()
        {
            var mods = _installedModsProvider.GetMods(this, ModTypeEnum.Autoload);

            return [.. mods];
        }


        /// <inheritdoc/>
        public void CreateCombinedMod()
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

            var files = _installedModsProvider.GetMods(this, ModTypeEnum.Autoload);

            if (files.Count == 0)
            {
                return;
            }

            foreach (var file in files)
            {
                file.ThrowIfNotType<AutoloadMod>(out var autoloadMod);
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

                using (var streamReader = new StreamReader(ini.Open()))
                {
                    newDef.Append(streamReader.ReadToEnd());
                    newDef.Append(Environment.NewLine);
                }
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
        protected abstract List<IMod> GetOriginalCampaigns();


        /// <summary>
        /// Do provided files exist in the game install folder
        /// </summary>
        /// <param name="files">List of required files</param>
        protected bool IsInstalled(List<string> files, string? path = null)
        {
            if (GameInstallFolder is null)
            {
                return false;
            }

            var gamePath = path is null ? GameInstallFolder : path;

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
