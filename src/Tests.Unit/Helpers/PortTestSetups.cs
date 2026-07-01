using Addons.Addons;
using Core.All.Enums;
using Core.All.Enums.Addons;
using Core.All.Enums.Versions;
using Core.All.Serializable.Addon;
using Core.Client.Helpers;
using Games.Games;

namespace Tests.Unit.Helpers;

/// <summary>
///     Provides pre-configured test setups for port testing across different games.
/// </summary>
internal static class PortTestSetups
{
    /// <summary>
    ///     Creates test setups for Blood game.
    /// </summary>
    internal static (BloodGame game, BloodCampaign baseCamp, BloodCampaign baseCampWithOptions, BloodCampaign cpCamp, BloodCampaign tcCamp, BloodCampaign tcFolderCamp, BloodCampaign tcExeOverride, BloodCampaign tcIncompatibleWithEnabled, BloodCampaign tcIncompatibleWithAll, LooseMap looseMap, AutoloadModsTestSetups mods) Blood()
    {
        var modsProvider = new AutoloadModsTestSetups(GameEnum.Blood);

        var game = new BloodGame
        {
            GameInstallFolder = Path.Combine("D:", "Games", "Blood")
        };

        var baseCamp = new BloodCampaign
        {
            AddonId = new(nameof(GameEnum.Blood).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Blood",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImageHash = null,
            INI = null,
            RFF = null,
            SND = null,
            Executables = null,
            Options = null
        };

        var baseCampWithOptions = new BloodCampaign
        {
            AddonId = new(nameof(GameEnum.Blood).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Blood",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImageHash = null,
            INI = null,
            RFF = null,
            SND = null,
            Executables = null,
            Options = new()
            {
                {
                    "option 1", new()
                    {
                        {
                            "OPT.DEF", OptionalParameterTypeEnum.DEF
                        }
                    }
                },
                {
                    "option 2", new()
                    {
                        {
                            "OPT2.DEF", OptionalParameterTypeEnum.DEF
                        },
                        {
                            "OPT2_2.DEF", OptionalParameterTypeEnum.DEF
                        }
                    }
                }
            }
        };

        var cpCamp = new BloodCampaign
        {
            AddonId = new(nameof(BloodAddonEnum.BloodCP).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Cryptic Passage",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = new Dictionary<string, string?>
            {
                {
                    nameof(BloodAddonEnum.BloodCP), null
                }
            },
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImageHash = null,
            INI = null,
            RFF = null,
            SND = null,
            Executables = null,
            Options = null
        };

        var tcCamp = new BloodCampaign
        {
            AddonId = new("blood-tc", null),
            Type = AddonTypeEnum.TC,
            Title = "Blood TC",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            FileInfo = new(Path.Combine("D:", "Games", "Blood", "blood_tc.zip"), "addon.json"),
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImageHash = null,
            INI = "TC.INI",
            RFF = "TC.RFF",
            SND = "TC.SND",
            Executables = null,
            Options = null
        };

        var tcFolderCamp = new BloodCampaign
        {
            AddonId = new("blood-tc-folder", null),
            Type = AddonTypeEnum.TC,
            Title = "Blood TC",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            FileInfo = new(Path.Combine("D:", "Games", "Blood", "blood_tc_folder"), "addon.json"),
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImageHash = null,
            INI = "TC.INI",
            RFF = "TC.RFF",
            SND = "TC.SND",
            Executables = null,
            Options = null
        };

        var tcExeOverride = new BloodCampaign
        {
            AddonId = new("blood-tc-exe-override", null),
            Type = AddonTypeEnum.TC,
            Title = "Blood TC",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            FileInfo = new(Path.Combine("D:", "Games", "Blood", "blood_tc_folder"), "addon.json"),
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImageHash = null,
            INI = "TC.INI",
            RFF = "TC.RFF",
            SND = "TC.SND",
            Executables = new Dictionary<OSEnum, Dictionary<PortEnum, string>>
            {
                {
                    OSEnum.Windows, new Dictionary<PortEnum, string>
                    {
                        {
                            PortEnum.NBlood, "nblood.exe"
                        }
                    }
                }
            },
            Options = null
        };

        var tcIncompatibleWithEnabled = new BloodCampaign
        {
            AddonId = new("blood-tc-imcompatible-with-enabled", null),
            Type = AddonTypeEnum.TC,
            Title = "Blood TC",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            FileInfo = new(Path.Combine("D:", "Games", "Blood", "blood_tc_folder"), "addon.json"),
            DependentAddons = null,
            IncompatibleAddons = new Dictionary<string, string?>
            {
                {
                    "enabledMod", null
                }
            },
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImageHash = null,
            INI = "TC.INI",
            RFF = "TC.RFF",
            SND = "TC.SND",
            Executables = null,
            Options = null
        };

        var tcIncompatibleWithAll = new BloodCampaign
        {
            AddonId = new("blood-tc-imcompatible-with-everything", null),
            Type = AddonTypeEnum.TC,
            Title = "Blood TC",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            FileInfo = new(Path.Combine("D:", "Games", "Blood", "blood_tc_folder"), "addon.json"),
            DependentAddons = null,
            IncompatibleAddons = new Dictionary<string, string?>
            {
                {
                    "*", null
                }
            },
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImageHash = null,
            INI = "TC.INI",
            RFF = "TC.RFF",
            SND = "TC.SND",
            Executables = null,
            Options = null
        };

        var looseMap = new LooseMap
        {
            AddonId = new("loose-map", null),
            Type = AddonTypeEnum.Map,
            Title = "Loose map",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            FileInfo = new("Maps", "LOOSE.MAP"),
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = new MapFileJsonModel
            {
                File = "LOOSE.MAP"
            },
            PreviewImageHash = null,
            Executables = null,
            BloodIni = null,
            Options = null
        };

        return (game, baseCamp, baseCampWithOptions, cpCamp, tcCamp, tcFolderCamp, tcExeOverride, tcIncompatibleWithEnabled, tcIncompatibleWithAll, looseMap, modsProvider);
    }

    /// <summary>
    ///     Creates test setups for Duke Nukem 3D game.
    /// </summary>
    internal static (DukeGame game, DukeCampaign baseCamp, DukeCampaign vacaCamp, DukeCampaign tcCamp, DukeCampaign wtCamp, DukeCampaign duke64Camp, DukeCampaign zhCamp, DukeCampaign dcCamp, DukeCampaign nwCamp, LooseMap looseMap, AutoloadModsTestSetups mods) Duke3D()
    {
        var modsProvider = new AutoloadModsTestSetups(GameEnum.Duke3D);

        var game = new DukeGame
        {
            Duke64RomPath = Path.Combine("D:", "Games", "Duke64", "rom.z64"),
            DukeZHRomPath = Path.Combine("D:", "Games", "DukeZH", "rom.z64"),
            DukeWTInstallPath = Path.Combine("D:", "Games", "DukeWT"),
            GameInstallFolder = Path.Combine("D:", "Games", "Duke3D"),
            AddonsPaths = new()
            {
                {
                    DukeAddonEnum.DukeVaca, Path.Combine("D:", "Games", "Duke3D", "Vaca")
                },
                {
                    DukeAddonEnum.DukeDC, Path.Combine("D:", "Games", "Duke3D", "DC")
                },
                {
                    DukeAddonEnum.DukeNW, Path.Combine("D:", "Games", "Duke3D", "NW")
                }
            }
        };

        var baseCamp = new DukeCampaign
        {
            AddonId = new(nameof(GameEnum.Duke3D).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Duke Nukem 3D",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null
        };

        var wtCamp = new DukeCampaign
        {
            AddonId = new(nameof(DukeVersionEnum.Duke3D_WT).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Duke Nukem 3D World Tour",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_WT),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null
        };

        var duke64Camp = new DukeCampaign
        {
            AddonId = new(nameof(GameEnum.Duke64).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Duke Nukem 64",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Duke64),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null
        };

        var zhCamp = new DukeCampaign
        {
            AddonId = new(nameof(GameEnum.DukeZeroHour).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Duke Nukem ZeroHour",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.DukeZeroHour),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null
        };

        var dcCamp = new DukeCampaign
        {
            AddonId = new(nameof(DukeAddonEnum.DukeDC).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Duke: DC",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = new Dictionary<string, string?>
            {
                {
                    nameof(DukeAddonEnum.DukeDC), null
                }
            },
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null
        };

        var nwCamp = new DukeCampaign
        {
            AddonId = new(nameof(DukeAddonEnum.DukeNW).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Duke: NW",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = new Dictionary<string, string?>
            {
                {
                    nameof(DukeAddonEnum.DukeNW), null
                }
            },
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null
        };

        var vacaCamp = new DukeCampaign
        {
            AddonId = new("dukevaca", null),
            Type = AddonTypeEnum.Official,
            Title = "Duke Nukem 3D Caribbean",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = new Dictionary<string, string?>
            {
                {
                    nameof(DukeAddonEnum.DukeVaca), null
                }
            },
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null
        };

        var tcCamp = new DukeCampaign
        {
            AddonId = new("duke-tc", "1.1"),
            Type = AddonTypeEnum.TC,
            Title = "Duke Nukem 3D TC",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            RequiredFeatures = null,
            FileInfo = new(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Duke3D", "Campaigns"), "duke_tc.zip"),
            DependentAddons = new Dictionary<string, string?>
            {
                {
                    nameof(DukeAddonEnum.DukeVaca), null
                }
            },
            IncompatibleAddons = null,
            MainCon = "TC.CON",
            AdditionalCons = ["TC1.CON", "TC2.CON"],
            MainDef = "TC.DEF",
            AdditionalDefs = ["TC1.DEF", "TC2.DEF"],
            RTS = "TC.RTS",
            StartMap = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null
        };

        var looseMap = new LooseMap
        {
            AddonId = new("loose-map", null),
            Type = AddonTypeEnum.Map,
            Title = "Loose map",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            RequiredFeatures = null,
            FileInfo = new("Maps", "LOOSE.MAP"),
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = new MapFileJsonModel
            {
                File = "LOOSE.MAP"
            },
            PreviewImageHash = null,
            Executables = null,
            BloodIni = null,
            Options = null
        };

        return (game, baseCamp, vacaCamp, tcCamp, wtCamp, duke64Camp, zhCamp, dcCamp, nwCamp, looseMap, modsProvider);
    }

    /// <summary>
    ///     Creates test setups for NAM game.
    /// </summary>
    internal static (NamGame game, DukeCampaign camp, AutoloadModsTestSetups mods) Nam()
    {
        var modsProvider = new AutoloadModsTestSetups(GameEnum.NAM);

        var game = new NamGame
        {
            GameInstallFolder = Path.Combine("D:", "Games", "NAM")
        };

        var camp = new DukeCampaign
        {
            AddonId = new(nameof(GameEnum.NAM).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "NAM",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.NAM),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null
        };

        return (game, camp, modsProvider);
    }

    /// <summary>
    ///     Creates test setups for Redneck Rampage game.
    /// </summary>
    internal static (RedneckGame game, DukeCampaign redneckCamp, DukeCampaign againCamp, DukeCampaign route66Camp, AutoloadModsTestSetups mods) Redneck()
    {
        var modsProvider = new AutoloadModsTestSetups(GameEnum.Redneck);

        var game = new RedneckGame
        {
            GameInstallFolder = Path.Combine("D:", "Games", "Redneck"),
            AgainInstallPath = Path.Combine("D:", "Games", "Again")
        };

        var redneckCamp = new DukeCampaign
        {
            AddonId = new(nameof(GameEnum.Redneck).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Redneck Rampage",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Redneck),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null
        };

        var againCamp = new DukeCampaign
        {
            AddonId = new(nameof(GameEnum.RidesAgain).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Rides Again",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.RidesAgain),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null
        };

        var route66Camp = new DukeCampaign
        {
            AddonId = new(nameof(RedneckAddonEnum.Route66).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Redneck Rampage Route 66",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Redneck),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null
        };

        return (game, redneckCamp, againCamp, route66Camp, modsProvider);
    }

    /// <summary>
    ///     Creates test setups for Slave (Exhumed/Powerslave) game.
    /// </summary>
    internal static (SlaveGame game, GenericCampaign camp, AutoloadModsTestSetups mods) Slave()
    {
        var modsProvider = new AutoloadModsTestSetups(GameEnum.Slave);

        var game = new SlaveGame
        {
            GameInstallFolder = Path.Combine("D:", "Games", "Slave")
        };

        var camp = new GenericCampaign
        {
            AddonId = new(nameof(GameEnum.Slave).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Slave",
            GridImageHash = null,
            PreviewImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Slave),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            Executables = null,
            Options = null
        };

        return (game, camp, modsProvider);
    }

    /// <summary>
    ///     Creates test setups for Shadow Warrior game.
    /// </summary>
    internal static (WangGame game, GenericCampaign wangCamp, GenericCampaign tdCamp, LooseMap looseMap, AutoloadModsTestSetups mods) Wang()
    {
        var modsProvider = new AutoloadModsTestSetups(GameEnum.Wang);

        var game = new WangGame
        {
            GameInstallFolder = Path.Combine("D:", "Games", "Wang")
        };

        var wangCamp = new GenericCampaign
        {
            AddonId = new(nameof(GameEnum.Wang).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Shadow Warrior",
            GridImageHash = null,
            PreviewImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Wang),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            Executables = null,
            Options = null
        };

        var tdCamp = new GenericCampaign
        {
            AddonId = new(nameof(WangAddonEnum.TwinDragon).ToLower(), null),
            Type = AddonTypeEnum.TC,
            Title = "Twin Dragon",
            GridImageHash = null,
            PreviewImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Wang),
            RequiredFeatures = null,
            FileInfo = new(Path.Combine("D:", "Games", "Wang"), "TD.zip"),
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            Executables = null,
            Options = null
        };

        var looseMap = new LooseMap
        {
            AddonId = new("loose-map", null),
            Type = AddonTypeEnum.Map,
            Title = "Loose map",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Wang),
            RequiredFeatures = null,
            FileInfo = new("Maps", "LOOSE.MAP"),
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = new MapFileJsonModel
            {
                File = "LOOSE.MAP"
            },
            PreviewImageHash = null,
            Executables = null,
            BloodIni = null,
            Options = null
        };

        return (game, wangCamp, tdCamp, looseMap, modsProvider);
    }

    /// <summary>
    ///     Creates test setups for World War II GI game.
    /// </summary>
    internal static (WW2GIGame game, DukeCampaign ww2Camp, DukeCampaign platoonCamp, AutoloadModsTestSetups mods) WW2GI()
    {
        var modsProvider = new AutoloadModsTestSetups(GameEnum.WW2GI);

        var game = new WW2GIGame
        {
            GameInstallFolder = Path.Combine("D:", "Games", "WW2GI")
        };

        var ww2Camp = new DukeCampaign
        {
            AddonId = new(nameof(GameEnum.WW2GI).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "World War II GI",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.WW2GI),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null
        };

        var platoonCamp = new DukeCampaign
        {
            AddonId = new(nameof(WW2GIAddonEnum.Platoon).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Platoon Leader",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.WW2GI),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null
        };

        return (game, ww2Camp, platoonCamp, modsProvider);
    }

    /// <summary>
    ///     Creates test setups for Ion Fury game.
    /// </summary>
    internal static (FuryGame game, DukeCampaign camp, AutoloadModsTestSetups mods) Fury()
    {
        var modsProvider = new AutoloadModsTestSetups(GameEnum.Fury);

        var game = new FuryGame
        {
            GameInstallFolder = Path.Combine("D:", "Games", "Fury")
        };

        var camp = new DukeCampaign
        {
            AddonId = new(nameof(GameEnum.Fury).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Ion Fury",
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Fury),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            RTS = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null
        };

        return (game, camp, modsProvider);
    }

    /// <summary>
    ///     Creates test setups for Witchaven game.
    /// </summary>
    internal static (WitchavenGame game, GenericCampaign camp) Witchaven()
    {
        var game = new WitchavenGame
        {
            GameInstallFolder = Path.Combine("D:", "Games", "Witchaven"),
            Witchaven2InstallPath = Path.Combine("D:", "Games", "Witchaven", "WH2")
        };

        var camp = new GenericCampaign
        {
            AddonId = new(nameof(GameEnum.Witchaven).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Witchaven",
            GridImageHash = null,
            PreviewImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.Witchaven),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            Executables = null,
            Options = null
        };

        return (game, camp);
    }

    /// <summary>
    ///     Creates test setups for TekWar game.
    /// </summary>
    internal static (TekWarGame game, GenericCampaign camp) TekWar()
    {
        var game = new TekWarGame
        {
            GameInstallFolder = Path.Combine("D:", "Games", "TekWar")
        };

        var camp = new GenericCampaign
        {
            AddonId = new(nameof(GameEnum.TekWar).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "TekWar",
            GridImageHash = null,
            PreviewImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            SupportedGame = new(GameEnum.TekWar),
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            Executables = null,
            Options = null
        };

        return (game, camp);
    }

    /// <summary>
    ///     Creates a Duke campaign with a packed zip addon.
    /// </summary>
    internal static DukeCampaign PackedDukeAddonCampaign()
    {
        var zipFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Duke3D", "Campaigns", "packed_campaign.zip");

        return new DukeCampaign
        {
            AddonId = new("packed-camp", "1.0"),
            Type = AddonTypeEnum.TC,
            Title = "Packed Campaign",
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            FileInfo = new AddonFilePathWrapper(zipFilePath, "addon.json"),
            DependentAddons = null,
            IncompatibleAddons = null,
            GridImageHash = null,
            PreviewImageHash = null,
            Description = null,
            Author = null,
            ReleaseDate = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            RequiredFeatures = null,
            Executables = null,
            Options = null
        };
    }
}
