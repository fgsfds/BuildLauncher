using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Mods.Mods;
using Mods.Providers;
using SharpCompress.Archives;
using System.IO.Compression;
using System.Reflection;
using ZipArchive = SharpCompress.Archives.Zip.ZipArchive;

namespace Games.Games
{
    public sealed class DukeGame : BaseGame
    {
        /// <inheritdoc/>
        public override GameEnum GameEnum => GameEnum.Duke3D;

        /// <inheritdoc/>
        public override string FullName => "Duke Nukem 3D";

        /// <inheritdoc/>
        public override string ShortName => "Duke3D";

        /// <summary>
        /// Path to Duke64 rom file
        /// </summary>
        public required string Duke64RomPath { get; set; }

        /// <summary>
        /// Path to World Tour folder
        /// </summary>
        public required string DukeWTInstallPath { get; set; }

        /// <inheritdoc/>
        public override string MainFile => "DUKE3D.GRP";

        /// <inheritdoc/>
        public override string DefFile => "duke3d.def";

        /// <inheritdoc/>
        public override List<string> RequiredFiles => [MainFile];

        /// <summary>
        /// List of files required for World Tour
        /// </summary>
        public List<string> RequiredFilesWorldTour => ["EPISODE5BOSS.CON", "FIREFLYTROOPER.CON", "FLAMETHROWER.CON"];

        /// <summary>
        /// Is Duke it Out in DC installed
        /// </summary>
        public bool IsDukeDCInstalled => IsInstalled("DUKEDC.GRP");

        /// <summary>
        /// Is Nuclear Winter installed
        /// </summary>
        public bool IsNuclearWinterInstalled => IsInstalled("NWINTER.GRP");

        /// <summary>
        /// Is Caribbean installed
        /// </summary>
        public bool IsCaribbeanInstalled => IsInstalled("VACATION.GRP");

        /// <summary>
        /// Is World Tour installed
        /// </summary>
        public bool IsWorldTourInstalled => IsInstalled(RequiredFilesWorldTour, DukeWTInstallPath);

        /// <summary>
        /// Is Duke 64 installed
        /// </summary>
        public bool IsDuke64Installed => File.Exists(Duke64RomPath);


        public DukeGame(InstalledModsProvider modsProvider) : base(modsProvider)
        {
            CreateWTStopgapFolder();
        }


        /// <inheritdoc/>
        protected override List<IMod> GetOriginalCampaigns()
        {
            List<IMod> campaigns = new(6);

            if (IsBaseGameInstalled)
            {
                campaigns.Add(new DukeCampaign()
                {
                    Guid = new(Consts.Duke3dGuid),
                    ModType = ModTypeEnum.Campaign,
                    DisplayName = "Duke Nukem 3D",
                    Image = ImageHelper.FileNameToStream("Duke3D.duke3d.jpg"),
                    StartupFile = null,
                    AddonEnum = DukeAddonEnum.Duke3D,
                    Version = null,
                    SupportedPorts = null,
                    Description = null,
                    Url = null,
                    Author = null,
                    IsOfficial = true,
                    PathToFile = null
                });

                if (IsCaribbeanInstalled)
                {
                    campaigns.Add(new DukeCampaign()
                    {
                        Guid = new(Consts.CaribbeanGuid),
                        ModType = ModTypeEnum.Campaign,
                        DisplayName = "Caribbean",
                        Image = ImageHelper.FileNameToStream("Duke3D.carib.jpg"),
                        StartupFile = null,
                        AddonEnum = DukeAddonEnum.Caribbean,
                        Version = null,
                        //TODO remove when https://voidpoint.io/terminx/eduke32/-/issues/297 is fixed
                        SupportedPorts = [PortEnum.Raze, PortEnum.BuildGDX],
                        Description = null,
                        Url = null,
                        Author = null,
                        IsOfficial = true,
                        PathToFile = null
                    });
                }
                if (IsNuclearWinterInstalled)
                {
                    campaigns.Add(new DukeCampaign()
                    {
                        Guid = new(Consts.NuclearWinterGuid),
                        ModType = ModTypeEnum.Campaign,
                        DisplayName = "Nuclear Winter",
                        Image = ImageHelper.FileNameToStream("Duke3D.nwinter.jpg"),
                        StartupFile = null,
                        AddonEnum = DukeAddonEnum.NuclearWinter,
                        Version = null,
                        SupportedPorts = null,
                        Description = null,
                        Url = null,
                        Author = null,
                        IsOfficial = true,
                        PathToFile = null
                    });
                }
                if (IsDukeDCInstalled)
                {
                    campaigns.Add(new DukeCampaign()
                    {
                        Guid = new(Consts.DukeDCGuid),
                        ModType = ModTypeEnum.Campaign,
                        DisplayName = "Duke it Out in DC",
                        Image = ImageHelper.FileNameToStream("Duke3D.dukedc.jpg"),
                        StartupFile = null,
                        AddonEnum = DukeAddonEnum.DukeDC,
                        Version = null,
                        //TODO remove when https://voidpoint.io/terminx/eduke32/-/issues/297 is fixed
                        SupportedPorts = [PortEnum.Raze, PortEnum.BuildGDX],
                        Description = null,
                        Url = null,
                        Author = null,
                        IsOfficial = true,
                        PathToFile = null
                    });
                }
            }

            if (IsWorldTourInstalled)
            {
                campaigns.Add(new DukeCampaign()
                {
                    Guid = new(Consts.WorldTourGuid),
                    ModType = ModTypeEnum.Campaign,
                    DisplayName = "Duke Nukem 3D World Tour",
                    Image = ImageHelper.FileNameToStream("Duke3D.dukewt.jpg"),
                    StartupFile = null,
                    AddonEnum = DukeAddonEnum.WorldTour,
                    SupportedPorts = [PortEnum.Raze, PortEnum.EDuke32, PortEnum.BuildGDX],
                    Version = null,
                    Description = null,
                    Url = null,
                    Author = null,
                    IsOfficial = true,
                    PathToFile = null
                });
            }

            if (IsDuke64Installed)
            {
                campaigns.Add(new DukeCampaign()
                {
                    Guid = new(Consts.Duke64Guid),
                    ModType = ModTypeEnum.Campaign,
                    DisplayName = "Duke Nukem 64",
                    Image = ImageHelper.FileNameToStream("Duke3D.duke64.jpg"),
                    StartupFile = null,
                    SupportedPorts = [PortEnum.RedNukem],
                    AddonEnum = DukeAddonEnum.Duke64,
                    Version = null,
                    Description = null,
                    Url = null,
                    Author = null,
                    IsOfficial = true,
                    PathToFile = null
                });
            }

            return campaigns;
        }


        /// <summary>
        /// Create folder with files required for World Tour to work with EDuke32
        /// </summary>
        private void CreateWTStopgapFolder()
        {
            var stopgapFolder = Path.Combine(SpecialFolderPath, Consts.WTStopgap);

            if (Directory.Exists(stopgapFolder))
            {
                return;
            }

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Games.Assets.WTStopgap.zip");

            stream.ThrowIfNull();

            using var archive = ZipArchive.Open(stream);

            archive.ExtractToDirectory(stopgapFolder);
        }
    }
}
