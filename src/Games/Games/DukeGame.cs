﻿using Common.Enums;
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
        public required string? Duke64RomPath { get; set; }

        /// <summary>
        /// Path to World Tour folder
        /// </summary>
        public required string? DukeWTInstallPath { get; set; }

        /// <inheritdoc/>
        public override string DefFile => "duke3d.def";

        /// <inheritdoc/>
        public override List<string> RequiredFiles => ["DUKE3D.GRP"];

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
        public bool IsWorldTourInstalled => IsInstalled(["EPISODE5BOSS.CON", "FIREFLYTROOPER.CON", "FLAMETHROWER.CON"], DukeWTInstallPath);

        /// <summary>
        /// Is Duke 64 installed
        /// </summary>
        public bool IsDuke64Installed => File.Exists(Duke64RomPath);


        public DukeGame(InstalledModsProviderFactory modsProvider, DownloadableModsProviderFactory downloadableModsProviderFactory) : base(modsProvider, downloadableModsProviderFactory)
        {
            CreateWTStopgapFolder();
        }


        /// <inheritdoc/>
        protected override Dictionary<Guid, IMod> GetOriginalCampaigns()
        {
            Dictionary<Guid, IMod> campaigns = new(6);

            if (IsBaseGameInstalled &&
                GameInstallFolder != DukeWTInstallPath)
            {
                campaigns.Add(Consts.Duke3dGuid, new DukeCampaign()
                {
                    Guid = Consts.Duke3dGuid,
                    ModType = ModTypeEnum.Campaign,
                    DisplayName = "Duke Nukem 3D",
                    Image = ImageHelper.FileNameToStream("Duke3D.duke3d.jpg"),
                    AddonEnum = DukeAddonEnum.Duke3D,
                    Author = "3D Realms",
                    Description = """
                        Duke Nukem 3D is a first-person shooter developed and published by **3D Realms**.
                        Released on April 19, 1996, Duke Nukem 3D is the third game in the Duke Nukem series and a sequel to Duke Nukem II.

                        The player assumes the role of Duke Nukem, an imperious action hero, and fights through 48 levels spread across 5 episodes. The player encounters a host of enemies and fights them with a range of weaponry.
                        In the end, Duke annihilates the alien overlords and celebrates by desecrating their corpses.
                        """,
                    StartupFile = null,
                    Version = null,
                    SupportedPorts = null,
                    Url = null,
                    IsOfficial = true,
                    PathToFile = null,
                    IsLoose = false
                });
            }

            if (IsWorldTourInstalled)
            {
                campaigns.Add(Consts.WorldTourGuid, new DukeCampaign()
                {
                    Guid = Consts.WorldTourGuid,
                    ModType = ModTypeEnum.Campaign,
                    DisplayName = "Duke Nukem 3D World Tour",
                    Image = ImageHelper.FileNameToStream("Duke3D.dukewt.jpg"),
                    AddonEnum = DukeAddonEnum.WorldTour,
                    SupportedPorts = [PortEnum.Raze, PortEnum.EDuke32, PortEnum.BuildGDX],
                    Author = "WizardWorks",
                    Description = """
                        **Duke Nukem 3D: 20th Anniversary World Tour** is a 2016 special edition remake of Duke Nukem 3D, originally released in 1996.
                        The remake includes all content from Duke Nukem 3D: Atomic Edition, but it adds new levels, enemies, a weapon, and several special features.

                        The 20th Anniversary Edition includes a new fifth episode known as Alien World Order.
                        The episode was designed by Allen Blum and Richard “Levelord” Gray, both of whom designed all the levels in the original Duke Nukem 3D. 
                        """,
                    StartupFile = null,
                    Version = null,
                    Url = null,
                    IsOfficial = true,
                    PathToFile = null,
                    IsLoose = false
                });
            }

            if (IsBaseGameInstalled)
            {
                if (IsCaribbeanInstalled)
                {
                    campaigns.Add(Consts.CaribbeanGuid, new DukeCampaign()
                    {
                        Guid = Consts.CaribbeanGuid,
                        ModType = ModTypeEnum.Campaign,
                        DisplayName = "Caribbean",
                        Image = ImageHelper.FileNameToStream("Duke3D.carib.jpg"),
                        AddonEnum = DukeAddonEnum.Caribbean,
                        Author = "Sunstorm Interactive",
                        Description = """
                            **Life's A Beach** is an expansion pack for the highly acclaimed first-person shooter Duke Nukem 3D. It was released on December 31, 1997 by **Sunstorm Interactive**.

                            Narrative elements in Duke Caribbean: Life's A Beach are sparse. According to the official game manual, Duke Nukem is on vacation in the Caribbean to take a break from killing aliens.
                            However, the aliens have decided that the Caribbean offers the perfect climate for a new breeding ground, so they begin laying eggs and terrorizing the local tourists.
                            Angered that his rest and relaxation is being delayed, Duke Nukem sets out on a mission for retribution against the aliens who are interrupting his vacation.
                            """,
                        StartupFile = null,
                        Version = null,
                        //TODO remove when https://voidpoint.io/terminx/eduke32/-/issues/297 is fixed
                        SupportedPorts = [PortEnum.Raze, PortEnum.BuildGDX],
                        Url = null,
                        IsOfficial = true,
                        PathToFile = null,
                        IsLoose = false
                    });
                }

                if (IsNuclearWinterInstalled)
                {
                    campaigns.Add(Consts.NuclearWinterGuid, new DukeCampaign()
                    {
                        Guid = Consts.NuclearWinterGuid,
                        ModType = ModTypeEnum.Campaign,
                        DisplayName = "Nuclear Winter",
                        Image = ImageHelper.FileNameToStream("Duke3D.nwinter.jpg"),
                        AddonEnum = DukeAddonEnum.NuclearWinter,
                        Author = "Simply Silly Software",
                        Description = """
                            **Nuclear Winter**, is a Christmas-themed expansion pack for Duke Nukem 3D. It was developed by **Simply Silly Software** and published by **WizardWorks** on December 30, 1997.

                            Santa Claus has been captured and brainwashed by the aliens that Duke previously defeated. To make matters worse, the aliens are now supported by an enemy force calling themselves the Feminist Elven Militia.
                            Duke Nukem must travel to the North Pole in order to stop the brainwashed Santa Claus and his manipulative captors.
                            """,
                        StartupFile = null,
                        Version = null,
                        SupportedPorts = null,
                        Url = null,
                        IsOfficial = true,
                        PathToFile = null,
                        IsLoose = false
                    });
                }

                if (IsDukeDCInstalled)
                {
                    campaigns.Add(Consts.DukeDCGuid, new DukeCampaign()
                    {
                        Guid = Consts.DukeDCGuid,
                        ModType = ModTypeEnum.Campaign,
                        DisplayName = "Duke it Out in DC",
                        Image = ImageHelper.FileNameToStream("Duke3D.dukedc.jpg"),
                        AddonEnum = DukeAddonEnum.DukeDC,
                        Author = "WizardWorks",
                        Description = """
                            **Duke It Out In D.C.** is a Duke Nukem 3D expansion pack developed by Sunstorm Interactive and published by **WizardWorks** on March 17, 1997.
                            The add-on does not introduce any new enemies, weapons, or sprites, but it features an all-new episode comprised of ten original levels,
                            each based on a famous location in Washington, D.C. 

                            Aliens have crash-landed into the Capitol Building and have launched a massive invasion of Washington, D.C. Duke Nukem arrives to find that the
                            alien invaders have captured several national monuments and critical government buildings, but in the end, Duke defeats the invading army and rescues the President from the Cycloid Emperor.
                            """,
                        StartupFile = null,
                        Version = null,
                        //TODO remove when https://voidpoint.io/terminx/eduke32/-/issues/297 is fixed
                        SupportedPorts = [PortEnum.Raze, PortEnum.BuildGDX],
                        Url = null,
                        IsOfficial = true,
                        PathToFile = null,
                        IsLoose = false
                    });
                }
            }

            if (IsDuke64Installed)
            {
                campaigns.Add(Consts.Duke64Guid, new DukeCampaign()
                {
                    Guid = Consts.Duke64Guid,
                    ModType = ModTypeEnum.Campaign,
                    DisplayName = "Duke Nukem 64",
                    Image = ImageHelper.FileNameToStream("Duke3D.duke64.jpg"),
                    AddonEnum = DukeAddonEnum.Duke64,
                    SupportedPorts = [PortEnum.RedNukem],
                    Author = "3D Realms, Eurocom",
                    Description = """
                        **Duke Nukem 64** is the Nintendo 64 port of the first-person shooter MS-DOS/PC game Duke Nukem 3D.
                        The Nintendo 64 port features significant changes from the PC version, including modified and expanded levels and a different set of weapons.
                        The port also includes a four-player deathmatch mode and a two-player co-op mode via split-screen.
                        The game's mature themes have been minimized to satisfy Nintendo's adult content standards.
                        """,
                    StartupFile = null,
                    Version = null,
                    Url = null,
                    IsOfficial = true,
                    PathToFile = null,
                    IsLoose = false
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
