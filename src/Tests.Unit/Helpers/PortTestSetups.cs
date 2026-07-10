using Addons.Addons;
using Addons.Providers;
using Core.All;
using Core.All.Enums;
using Core.All.Enums.Addons;
using Core.All.Enums.Versions;
using Core.All.Serializable.Addon;
using Core.Client.Config;
using Core.Client.Helpers;
using Games.Games;

namespace Tests.Unit.Helpers;

/// <summary>
///     Provides pre-configured test setups for port testing across different games.
/// </summary>
internal static class PortTestSetups
{
    private static readonly OriginalCampaignsProvider _provider = new(new ConfigProviderFake());
    private static readonly string TestDataRoot = Path.Combine(Directory.GetCurrentDirectory(), "Data", "TestSetups");

    private static void CreateFiles(string dir, params string[] files)
    {
        foreach (var file in files)
        {
            var path = Path.Combine(dir, file);
            var parent = Path.GetDirectoryName(path);
            if (parent is not null && !Directory.Exists(parent))
            {
                _ = Directory.CreateDirectory(parent);
            }

            if (!File.Exists(path))
            {
                using (File.Create(path)) { }
            }
        }
    }

    private static void CreateNumberedFiles(string dir, string baseName, string extension, int start, int endExclusive, int padWidth)
    {
        for (var i = start; i < endExclusive; i++)
        {
            var fileName = padWidth > 0
                ? $"{baseName}{i.ToString().PadLeft(padWidth, '0')}.{extension}"
                : $"{baseName}{i}.{extension}";

            var path = Path.Combine(dir, fileName);
            if (!File.Exists(path))
            {
                using (File.Create(path)) { }
            }
        }
    }
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

        var testDir = Path.Combine(TestDataRoot, "Blood");
        Directory.CreateDirectory(testDir);
        CreateFiles(testDir,
            ClientConsts.BloodIni, ClientConsts.BloodRff, ClientConsts.BloodSnd,
            "GUI.RFF", "SURFACE.DAT", "TILES000.ART", "VOXEL.DAT",
            ClientConsts.CrypticIni, "CP01.MAP", "CPART07.AR_", "CPART15.AR_",
            "CRYPTIC.SMK", "CRYPTIC.WAV");

        var originalFolder = game.GameInstallFolder;
        game.GameInstallFolder = testDir;

        Dictionary<AddonId, BaseAddon> campaigns;
        try
        {
            campaigns = _provider.GetOriginalCampaigns(game);
        }
        finally
        {
            game.GameInstallFolder = originalFolder;
        }

        var baseCamp = (BloodCampaign)campaigns[new AddonId(nameof(GameEnum.Blood).ToLower(), null)];

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

        var cpCamp = (BloodCampaign)campaigns[new AddonId(nameof(BloodAddonEnum.BloodCP).ToLower(), null)];

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

        var testDir = Path.Combine(TestDataRoot, "Duke3D");
        var wtTestDir = Path.Combine(testDir, "WorldTour");
        var duke64TestDir = Path.Combine(testDir, "Duke64");
        var dukeZhTestDir = Path.Combine(testDir, "DukeZH");

        Directory.CreateDirectory(testDir);
        Directory.CreateDirectory(wtTestDir);
        Directory.CreateDirectory(duke64TestDir);
        Directory.CreateDirectory(dukeZhTestDir);

        CreateFiles(testDir,
            "DUKE3D.GRP",
            "VACATION.GRP",
            "DUKEDC.GRP",
            "NWINTER.GRP");
        CreateFiles(wtTestDir,
            "EPISODE5BOSS.CON",
            "FIREFLYTROOPER.CON",
            "FLAMETHROWER.CON");
        if (!File.Exists(Path.Combine(duke64TestDir, "rom.z64")))
        {
            using (File.Create(Path.Combine(duke64TestDir, "rom.z64"))) { }
        }

        if (!File.Exists(Path.Combine(dukeZhTestDir, "rom.z64")))
        {
            using (File.Create(Path.Combine(dukeZhTestDir, "rom.z64"))) { }
        }

        var originalFolder = game.GameInstallFolder;
        var originalWtPath = game.DukeWTInstallPath;
        var original64Path = game.Duke64RomPath;
        var originalZhPath = game.DukeZHRomPath;
        var savedAddonsPaths = new Dictionary<DukeAddonEnum, string>(game.AddonsPaths);

        game.GameInstallFolder = testDir;
        game.DukeWTInstallPath = wtTestDir;
        game.Duke64RomPath = Path.Combine(duke64TestDir, "rom.z64");
        game.DukeZHRomPath = Path.Combine(dukeZhTestDir, "rom.z64");

        Dictionary<AddonId, BaseAddon> campaigns;
        try
        {
            campaigns = _provider.GetOriginalCampaigns(game);
        }
        finally
        {
            game.GameInstallFolder = originalFolder;
            game.DukeWTInstallPath = originalWtPath;
            game.Duke64RomPath = original64Path;
            game.DukeZHRomPath = originalZhPath;

            game.AddonsPaths.Clear();
            foreach (var kvp in savedAddonsPaths)
            {
                game.AddonsPaths[kvp.Key] = kvp.Value;
            }
        }

        var baseCamp = (DukeCampaign)campaigns[new AddonId(nameof(GameEnum.Duke3D).ToLower(), null)];
        var vacaCamp = (DukeCampaign)campaigns[new AddonId(nameof(DukeAddonEnum.DukeVaca).ToLower(), null)];
        var wtCamp = (DukeCampaign)campaigns[new AddonId(nameof(DukeVersionEnum.Duke3D_WT).ToLower(), null)];
        var duke64Camp = (DukeCampaign)campaigns[new AddonId(nameof(GameEnum.Duke64).ToLower(), null)];
        var zhCamp = (DukeCampaign)campaigns[new AddonId(nameof(GameEnum.DukeZeroHour).ToLower(), null)];
        var dcCamp = (DukeCampaign)campaigns[new AddonId(nameof(DukeAddonEnum.DukeDC).ToLower(), null)];
        var nwCamp = (DukeCampaign)campaigns[new AddonId(nameof(DukeAddonEnum.DukeNW).ToLower(), null)];

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
            AdditionalCons =
            [
                "TC1.CON",
                "TC2.CON"
            ],
            MainDef = "TC.DEF",
            AdditionalDefs =
            [
                "TC1.DEF",
                "TC2.DEF"
            ],
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

        var testDir = Path.Combine(TestDataRoot, "NAM");
        Directory.CreateDirectory(testDir);
        CreateFiles(testDir, "NAM.GRP");

        var originalFolder = game.GameInstallFolder;
        game.GameInstallFolder = testDir;

        Dictionary<AddonId, BaseAddon> campaigns;
        try
        {
            campaigns = _provider.GetOriginalCampaigns(game);
        }
        finally
        {
            game.GameInstallFolder = originalFolder;
        }

        var camp = (DukeCampaign)campaigns[new AddonId(nameof(GameEnum.NAM).ToLower(), null)];

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

        var testDir = Path.Combine(TestDataRoot, "Redneck");
        var againTestDir = Path.Combine(testDir, "Again");
        Directory.CreateDirectory(testDir);
        Directory.CreateDirectory(againTestDir);
        CreateFiles(testDir, "REDNECK.GRP",
            "TILESA66.ART", "TILESB66.ART", "TURD66.ANM", "TURD66.VOC",
            "END66.ANM", "END66.VOC", "BUBBA66.CON", "DEFS66.CON",
            "GATOR66.CON", "GAME66.CON", "PIG66.CON");
        CreateFiles(againTestDir, "REDNECK.GRP", "BIKER.CON");

        var originalFolder = game.GameInstallFolder;
        var originalAgainPath = game.AgainInstallPath;
        game.GameInstallFolder = testDir;
        game.AgainInstallPath = againTestDir;

        Dictionary<AddonId, BaseAddon> campaigns;
        try
        {
            campaigns = _provider.GetOriginalCampaigns(game);
        }
        finally
        {
            game.GameInstallFolder = originalFolder;
            game.AgainInstallPath = originalAgainPath;
        }

        var redneckCamp = (DukeCampaign)campaigns[new AddonId(nameof(GameEnum.Redneck).ToLower(), null)];
        var againCamp = (DukeCampaign)campaigns[new AddonId(nameof(GameEnum.RidesAgain).ToLower(), null)];
        var route66Camp = (DukeCampaign)campaigns[new AddonId(nameof(RedneckAddonEnum.Route66).ToLower(), null)];

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

        var testDir = Path.Combine(TestDataRoot, "Slave");
        Directory.CreateDirectory(testDir);
        CreateFiles(testDir, "STUFF.DAT");

        var originalFolder = game.GameInstallFolder;
        game.GameInstallFolder = testDir;

        Dictionary<AddonId, BaseAddon> campaigns;
        try
        {
            campaigns = _provider.GetOriginalCampaigns(game);
        }
        finally
        {
            game.GameInstallFolder = originalFolder;
        }

        var camp = (GenericCampaign)campaigns[new AddonId(nameof(GameEnum.Slave).ToLower(), null)];

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

        var testDir = Path.Combine(TestDataRoot, "Wang");
        Directory.CreateDirectory(testDir);
        CreateFiles(testDir, "SW.GRP");

        var originalFolder = game.GameInstallFolder;
        game.GameInstallFolder = testDir;

        Dictionary<AddonId, BaseAddon> campaigns;
        try
        {
            campaigns = _provider.GetOriginalCampaigns(game);
        }
        finally
        {
            game.GameInstallFolder = originalFolder;
        }

        var wangCamp = (GenericCampaign)campaigns[new AddonId(nameof(GameEnum.Wang).ToLower(), null)];

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

        var testDir = Path.Combine(TestDataRoot, "WW2GI");
        Directory.CreateDirectory(testDir);
        CreateFiles(testDir, "WW2GI.GRP", "PLATOONL.DAT", "PLATOONL.DEF");

        var originalFolder = game.GameInstallFolder;
        game.GameInstallFolder = testDir;

        Dictionary<AddonId, BaseAddon> campaigns;
        try
        {
            campaigns = _provider.GetOriginalCampaigns(game);
        }
        finally
        {
            game.GameInstallFolder = originalFolder;
        }

        var ww2Camp = (DukeCampaign)campaigns[new AddonId(nameof(GameEnum.WW2GI).ToLower(), null)];
        var platoonCamp = (DukeCampaign)campaigns[new AddonId(nameof(WW2GIAddonEnum.Platoon).ToLower(), null)];

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

        var testDir = Path.Combine(TestDataRoot, "Fury");
        Directory.CreateDirectory(testDir);
        CreateFiles(testDir, "fury.grp");

        var originalFolder = game.GameInstallFolder;
        game.GameInstallFolder = testDir;

        Dictionary<AddonId, BaseAddon> campaigns;
        try
        {
            campaigns = _provider.GetOriginalCampaigns(game);
        }
        finally
        {
            game.GameInstallFolder = originalFolder;
        }

        var camp = (DukeCampaign)campaigns[new AddonId(nameof(GameEnum.Fury).ToLower(), null)];

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

        var testDir = Path.Combine(TestDataRoot, "Witchaven");
        Directory.CreateDirectory(testDir);
        CreateFiles(testDir, "JOESND", "SONGS");
        CreateNumberedFiles(testDir, "TILES", "ART", 0, 11, 3);
        CreateNumberedFiles(testDir, "LEVEL", "MAP", 1, 26, 0);

        var originalFolder = game.GameInstallFolder;
        game.GameInstallFolder = testDir;

        Dictionary<AddonId, BaseAddon> campaigns;
        try
        {
            campaigns = _provider.GetOriginalCampaigns(game);
        }
        finally
        {
            game.GameInstallFolder = originalFolder;
        }

        var camp = (GenericCampaign)campaigns[new AddonId(nameof(GameEnum.Witchaven).ToLower(), null)];

        return (game, camp);
    }

    /// <summary>
    ///     Creates test setups for TekWar game.
    /// </summary>
    internal static (TekWarGame game, DukeCampaign camp) TekWar()
    {
        var game = new TekWarGame
        {
            GameInstallFolder = Path.Combine("D:", "Games", "TekWar")
        };

        var testDir = Path.Combine(TestDataRoot, "TekWar");
        Directory.CreateDirectory(testDir);
        CreateFiles(testDir, "SONGS", "SOUNDS");
        CreateNumberedFiles(testDir, "TILES", "ART", 0, 16, 3);

        var originalFolder = game.GameInstallFolder;
        game.GameInstallFolder = testDir;

        Dictionary<AddonId, BaseAddon> campaigns;
        try
        {
            campaigns = _provider.GetOriginalCampaigns(game);
        }
        finally
        {
            game.GameInstallFolder = originalFolder;
        }

        var camp = (DukeCampaign)campaigns[new AddonId(nameof(GameEnum.TekWar).ToLower(), null)];

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
