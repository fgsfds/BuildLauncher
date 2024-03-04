using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Mods.Providers;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.Collections.Immutable;
using System.IO.Compression;
using System.Text;
using ZipArchive = SharpCompress.Archives.Zip.ZipArchive;

namespace Games.Games
{
    /// <summary>
    /// Base class that encapsulates logic for working with games and their mods
    /// </summary>
    public abstract class BaseGame : IGame
    {
        protected readonly InstalledModsProvider _modsProvider;

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
            _modsProvider = modsProvider;

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

            var cusomCampaigns = _modsProvider.GetMods(this, ModTypeEnum.Campaign);

            return [.. campaigns, .. cusomCampaigns];
        }


        /// <inheritdoc/>
        public virtual ImmutableList<IMod> GetSingleMaps()
        {
            var maps = _modsProvider.GetMods(this, ModTypeEnum.Map);

            return [.. maps];
        }


        /// <inheritdoc/>
        public virtual ImmutableList<IMod> GetAutoloadMods()
        {
            var mods = _modsProvider.GetMods(this, ModTypeEnum.Autoload);

            return [.. mods];
        }


        /// <inheritdoc/>
        public void CreateCombinedMod()
        {
            if (!Directory.Exists(ModsFolderPath))
            {
                return;
            }

            var filePath = Path.Combine(SpecialFolderPath, Consts.CombinedMod);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            string newDef = string.Empty;
            var files = Directory.GetFiles(ModsFolderPath);

            if (files.Length == 0)
            {
                return;
            }

            foreach (var file in files)
            {
                using var zip = ZipFile.OpenRead(file);

                var ini = zip.Entries.FirstOrDefault(x => x.Name.Equals(DefFile));

                if (ini is null)
                {
                    continue;
                }

                using (var streamReader = new StreamReader(ini.Open()))
                {
                    newDef += streamReader.ReadToEnd() + Environment.NewLine;
                }
            }

            using var memStream = new MemoryStream(Encoding.UTF8.GetBytes(newDef));

            if (!Directory.Exists(SpecialFolderPath))
            {
                Directory.CreateDirectory(SpecialFolderPath);
            }

            using (var archive = ZipArchive.Create())
            {
                archive.AddEntry(DefFile, memStream);

                archive.SaveTo(filePath, CompressionType.None);
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
