using Common;
using Common.Enums;
using Common.Enums.Addons;
using Common.Enums.Versions;
using Common.Helpers;
using Common.Interfaces;
using Mods.Addons;
using SharpCompress.Archives;
using System.IO.Compression;
using System.Reflection;
using ZipArchive = SharpCompress.Archives.Zip.ZipArchive;

namespace Games.Games;

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


    public DukeGame() : base()
    {
        CreateWTStopgapFolder();
    }


    /// <inheritdoc/>
    public override Dictionary<AddonVersion, IAddon> GetOriginalCampaigns()
    {
        Dictionary<AddonVersion, IAddon> campaigns = new(6);

        if (IsBaseGameInstalled &&
            GameInstallFolder != DukeWTInstallPath)
        {
            var dukeId = nameof(GameEnum.Duke3D).ToLower();
            campaigns.Add(new(dukeId), new DukeCampaign()
            {
                Id = dukeId,
                Type = AddonTypeEnum.Official,
                Title = "Duke Nukem 3D",
                GridImage = ImageHelper.FileNameToStream("Duke3D.duke3d.jpg", Assembly.GetExecutingAssembly()),
                Author = "3D Realms",
                Description = """
                    Duke Nukem 3D is a first-person shooter developed and published by **3D Realms**.
                    Released on April 19, 1996, Duke Nukem 3D is the third game in the Duke Nukem series and a sequel to Duke Nukem II.

                    The player assumes the role of Duke Nukem, an imperious action hero, and fights through 48 levels spread across 5 episodes. The player encounters a host of enemies and fights them with a range of weaponry.
                    In the end, Duke annihilates the alien overlords and celebrates by desecrating their corpses.
                    """,
                Version = null,
                SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainCon = null,
                AdditionalCons = null,
                MainDef = null,
                AdditionalDefs = null,
                RTS = null,
                StartMap = null,
                PreviewImage = null
            });
        }

        if (IsWorldTourInstalled)
        {
            var dukeWtId = nameof(DukeVersionEnum.Duke3D_WT).ToLower();
            campaigns.Add(new(dukeWtId), new DukeCampaign()
            {
                Id = dukeWtId,
                Type = AddonTypeEnum.Official,
                Title = "Duke Nukem 3D World Tour",
                GridImage = ImageHelper.FileNameToStream("Duke3D.dukewt.jpg", Assembly.GetExecutingAssembly()),
                Author = "Nerve Software, Gearbox Software",
                Description = """
                    **Duke Nukem 3D: 20th Anniversary World Tour** is a 2016 special edition of Duke Nukem 3D.
                    This edition includes all content from Duke Nukem 3D: Atomic Edition, but it adds new levels, enemies, a weapon, and several special features.

                    The 20th Anniversary Edition includes a new fifth episode known as Alien World Order.
                    The episode was designed by Allen Blum and Richard “Levelord” Gray, both of whom designed all the levels in the original Duke Nukem 3D. 
                    """,
                Version = null,
                SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_WT),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainCon = null,
                AdditionalCons = null,
                MainDef = null,
                AdditionalDefs = null,
                RTS = null,
                StartMap = null,
                PreviewImage = null
            });
        }

        if (IsBaseGameInstalled)
        {
            if (IsCaribbeanInstalled)
            {
                var dukeVacaId = nameof(DukeAddonEnum.DukeVaca).ToLower();
                campaigns.Add(new(dukeVacaId), new DukeCampaign()
                {
                    Id = dukeVacaId,
                    Type = AddonTypeEnum.Official,
                    Title = "Caribbean",
                    GridImage = ImageHelper.FileNameToStream("Duke3D.carib.jpg", Assembly.GetExecutingAssembly()),
                    Author = "Sunstorm Interactive",
                    Description = """
                        **Life's A Beach** is an expansion pack for the highly acclaimed first-person shooter Duke Nukem 3D. It was released on December 31, 1997 by **Sunstorm Interactive**.

                        Narrative elements in Duke Caribbean: Life's A Beach are sparse. According to the official game manual, Duke Nukem is on vacation in the Caribbean to take a break from killing aliens.
                        However, the aliens have decided that the Caribbean offers the perfect climate for a new breeding ground, so they begin laying eggs and terrorizing the local tourists.
                        Angered that his rest and relaxation is being delayed, Duke Nukem sets out on a mission for retribution against the aliens who are interrupting his vacation.
                        """,
                    Version = null,
                    SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
                    RequiredFeatures = null,
                    PathToFile = null,
                    DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { nameof(DukeAddonEnum.DukeVaca), null } },
                    IncompatibleAddons = null,
                    MainCon = null,
                    AdditionalCons = null,
                    MainDef = null,
                    AdditionalDefs = null,
                    RTS = null,
                    StartMap = null,
                    PreviewImage = null
                });
            }

            if (IsNuclearWinterInstalled)
            {
                var dukeNwId = nameof(DukeAddonEnum.DukeNW).ToLower();
                campaigns.Add(new(dukeNwId), new DukeCampaign()
                {
                    Id = dukeNwId,
                    Type = AddonTypeEnum.Official,
                    Title = "Nuclear Winter",
                    GridImage = ImageHelper.FileNameToStream("Duke3D.nwinter.jpg", Assembly.GetExecutingAssembly()),
                    Author = "Simply Silly Software",
                    Description = """
                        **Nuclear Winter**, is a Christmas-themed expansion pack for Duke Nukem 3D. It was developed by **Simply Silly Software** and published by **WizardWorks** on December 30, 1997.

                        Santa Claus has been captured and brainwashed by the aliens that Duke previously defeated. To make matters worse, the aliens are now supported by an enemy force calling themselves the Feminist Elven Militia.
                        Duke Nukem must travel to the North Pole in order to stop the brainwashed Santa Claus and his manipulative captors.
                        """,
                    Version = null,
                    SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
                    RequiredFeatures = null,
                    PathToFile = null,
                    DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { nameof(DukeAddonEnum.DukeNW), null } },
                    IncompatibleAddons = null,
                    MainCon = null,
                    AdditionalCons = null,
                    MainDef = null,
                    AdditionalDefs = null,
                    RTS = null,
                    StartMap = null,
                    PreviewImage = null
                });
            }

            if (IsDukeDCInstalled)
            {
                var dukeDcId = nameof(DukeAddonEnum.DukeDC).ToLower();
                campaigns.Add(new(dukeDcId), new DukeCampaign()
                {
                    Id = dukeDcId,
                    Type = AddonTypeEnum.Official,
                    Title = "Duke it Out in DC",
                    GridImage = ImageHelper.FileNameToStream("Duke3D.dukedc.jpg", Assembly.GetExecutingAssembly()),
                    Author = "WizardWorks",
                    Description = """
                        **Duke It Out In D.C.** is a Duke Nukem 3D expansion pack developed by Sunstorm Interactive and published by **WizardWorks** on March 17, 1997.
                        The add-on does not introduce any new enemies, weapons, or sprites, but it features an all-new episode comprised of ten original levels,
                        each based on a famous location in Washington, D.C. 

                        Aliens have crash-landed into the Capitol Building and have launched a massive invasion of Washington, D.C. Duke Nukem arrives to find that the
                        alien invaders have captured several national monuments and critical government buildings, but in the end, Duke defeats the invading army and rescues the President from the Cycloid Emperor.
                        """,
                    Version = null,
                    SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
                    RequiredFeatures = null,
                    PathToFile = null,
                    DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { nameof(DukeAddonEnum.DukeDC), null } },
                    IncompatibleAddons = null,
                    MainCon = null,
                    AdditionalCons = null,
                    MainDef = null,
                    AdditionalDefs = null,
                    RTS = null,
                    StartMap = null,
                    PreviewImage = null
                });
            }
        }

        if (IsDuke64Installed)
        {
            var duke64Id = nameof(GameEnum.Duke64).ToLower();
            campaigns.Add(new(duke64Id), new DukeCampaign()
            {
                Id = duke64Id,
                Type = AddonTypeEnum.Official,
                Title = "Duke Nukem 64",
                GridImage = ImageHelper.FileNameToStream("Duke3D.duke64.jpg", Assembly.GetExecutingAssembly()),
                Author = "3D Realms, Eurocom",
                Description = """
                    **Duke Nukem 64** is the Nintendo 64 port of the first-person shooter MS-DOS/PC game Duke Nukem 3D.
                    The Nintendo 64 port features significant changes from the PC version, including modified and expanded levels and a different set of weapons.
                    The port also includes a four-player deathmatch mode and a two-player co-op mode via split-screen.
                    The game's mature themes have been minimized to satisfy Nintendo's adult content standards.
                    """,
                Version = null,
                SupportedGame = new(GameEnum.Duke64),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainCon = null,
                AdditionalCons = null,
                MainDef = null,
                AdditionalDefs = null,
                RTS = null,
                StartMap = null,
                PreviewImage = null
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
