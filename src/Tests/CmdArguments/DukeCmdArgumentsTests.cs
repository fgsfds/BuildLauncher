using Addons.Addons;
using Common.All;
using Common.All.Enums;
using Common.All.Enums.Addons;
using Common.All.Enums.Versions;
using Games.Games;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Tests.CmdArguments;

[Collection("Sync")]
public sealed class DukeCmdArgumentsTests
{
    private readonly DukeGame _dukeGame;
    private readonly DukeCampaign _dukeCamp;
    private readonly DukeCampaign _dukeVaca;
    private readonly DukeCampaign _dukeTcForVaca;
    private readonly DukeCampaign _dukeWtCamp;
    private readonly DukeCampaign _duke64Camp;

    private readonly AutoloadModsProvider _modsProvider;

    public DukeCmdArgumentsTests()
    {
        _modsProvider = new(GameEnum.Duke3D);

        _dukeGame = new()
        {
            Duke64RomPath = Path.Combine("D:", "Games", "Duke64", "rom.z64"),
            DukeWTInstallPath = Path.Combine("D:", "Games", "DukeWT"),
            GameInstallFolder = Path.Combine("D:", "Games", "Duke3D"),
            AddonsPaths = new() { { DukeAddonEnum.DukeVaca, Path.Combine("D:", "Games", "Duke3D", "Vaca") } }
        };

        _dukeCamp = new()
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
            PathToFile = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImageHash = null,
            IsUnpacked = false,
            Executables = null,
            Options = null
        };

        _dukeWtCamp = new()
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
            PathToFile = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImageHash = null,
            IsUnpacked = false,
            Executables = null,
            Options = null
        };

        _duke64Camp = new()
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
            PathToFile = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImageHash = null,
            IsUnpacked = false,
            Executables = null,
            Options = null
        };

        _dukeVaca = new()
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
            PathToFile = null,
            DependentAddons = new Dictionary<string, string?>() { { nameof(DukeAddonEnum.DukeVaca), null } },
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImageHash = null,
            IsUnpacked = false,
            Executables = null,
            Options = null
        };

        _dukeTcForVaca = new()
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
            PathToFile = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Duke3D", "Campaigns", "duke_tc.zip"),
            DependentAddons = new Dictionary<string, string?>() { { nameof(DukeAddonEnum.DukeVaca), null } },
            IncompatibleAddons = null,
            MainCon = "TC.CON",
            RTS = "TC.RTS",
            AdditionalCons = ["TC1.CON", "TC2.CON"],
            MainDef = "TC.DEF",
            AdditionalDefs = ["TC1.DEF", "TC2.DEF"],
            StartMap = null,
            PreviewImageHash = null,
            IsUnpacked = false,
            Executables = null,
            Options = null
        };

    }

    [Fact]
    public void RazeTest()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledModWithCons,
            _modsProvider.DisabledMod,
            _modsProvider.ModThatRequiresOfficialAddon,
            _modsProvider.ModThatIncompatibleWithAddon,
            _modsProvider.IncompatibleMod,
            _modsProvider.IncompatibleModWithIncompatibleVersion,
            _modsProvider.IncompatibleModWithCompatibleVersion,
            _modsProvider.DependentMod,
            _modsProvider.DependentModWithCompatibleVersion,
            _modsProvider.DependentModWithIncompatibleVersion,
            _modsProvider.ModForAnotherGame,
            _modsProvider.ModThatRequiresFeature,
            _modsProvider.MultipleDependenciesMod
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        Raze raze = new();

        raze.BeforeStart(_dukeGame, _dukeCamp);
        var args = raze.GetStartGameArgs(_dukeGame, _dukeCamp, mods, [], true, true);
        var expected = $"" +
            $" -file \"enabled_mod.zip\"" +
            $" -adddef \"ENABLED1.DEF\"" +
            $" -adddef \"ENABLED2.DEF\"" +
            $" -addcon \"ENABLED1.CON\"" +
            $" -addcon \"ENABLED2.CON\"" +
            $" -file \"mod_incompatible_with_addon.zip\"" +
            $" -file \"incompatible_mod_with_compatible_version.zip\"" +
            $" -file \"dependent_mod.zip\"" +
            $" -file \"dependent_mod_with_compatible_version.zip\"" +
            $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Duke3D\\duke3d\"" +
            $" -def \"a\"" +
            $" -addon 0" +
            $" -quick" +
            $" -nosetup" +
            $"";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
            [GameSearch.Directories]
            Path=D:/Games/Duke3D
            Path=D:/Games/Duke3D/Vaca

            [FileSearch.Directories]
            Path={Directory.GetCurrentDirectory()}/Data/Addons/Duke3D/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void RazeWtTest()
    {
        Raze raze = new();

        var args = raze.GetStartGameArgs(_dukeGame, _dukeWtCamp, new Dictionary<AddonId, BaseAddon>(), [], true, true);
        var expected = $"" +
            $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Duke3D\\duke3d_wt\"" +
            $" -def \"a\"" +
            $" -addon 0" +
            $" -quick" +
            $" -nosetup" +
            $"";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
            [GameSearch.Directories]
            Path=D:/Games/DukeWT

            [FileSearch.Directories]
            Path={Directory.GetCurrentDirectory()}/Data/Addons/Duke3D/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void RazeVacaTest()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledModWithCons,
            _modsProvider.ModThatRequiresOfficialAddon,
            _modsProvider.ModThatIncompatibleWithAddon
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        Raze raze = new();

        raze.BeforeStart(_dukeGame, _dukeVaca);
        var args = raze.GetStartGameArgs(_dukeGame, _dukeVaca, mods, [], true, true);
        var expected = $"" +
            $" -file \"enabled_mod.zip\"" +
            $" -adddef \"ENABLED1.DEF\"" +
            $" -adddef \"ENABLED2.DEF\"" +
            $" -addcon \"ENABLED1.CON\"" +
            $" -addcon \"ENABLED2.CON\"" +
            $" -file \"mod_requires_addon.zip\"" +
            $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Duke3D\\dukevaca\"" +
            $" -def \"a\"" +
            $" -addon 3" +
            $" -quick" +
            $" -nosetup" +
            $"";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
            [GameSearch.Directories]
            Path=D:/Games/Duke3D
            Path=D:/Games/Duke3D/Vaca

            [FileSearch.Directories]
            Path={Directory.GetCurrentDirectory()}/Data/Addons/Duke3D/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void RazeTCTest()
    {
        Raze raze = new();

        var args = raze.GetStartGameArgs(_dukeGame, _dukeTcForVaca, new Dictionary<AddonId, BaseAddon>(), [], true, true);
        var expected = $"" +
            $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Duke3D\\duke-tc\"" +
            $" -def \"TC.DEF\"" +
            $" -adddef \"TC1.DEF\"" +
            $" -adddef \"TC2.DEF\"" +
            $" -addon 3" +
            $" -con \"TC.CON\"" +
            $" -addcon \"TC1.CON\"" +
            $" -addcon \"TC2.CON\"" +
            $" -file \"{Path.Combine(Directory.GetCurrentDirectory(), "Data", "Duke3D", "Campaigns", "duke_tc.zip")}\"" +
            $" -quick" +
            $" -nosetup" +
            $"";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void EDuke32Test()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledModWithCons,
            _modsProvider.DisabledMod,
            _modsProvider.ModThatRequiresOfficialAddon,
            _modsProvider.ModThatIncompatibleWithAddon,
            _modsProvider.IncompatibleMod,
            _modsProvider.IncompatibleModWithIncompatibleVersion,
            _modsProvider.IncompatibleModWithCompatibleVersion,
            _modsProvider.DependentMod,
            _modsProvider.DependentModWithCompatibleVersion,
            _modsProvider.DependentModWithIncompatibleVersion,
            _modsProvider.ModForAnotherGame,
            _modsProvider.ModThatRequiresFeature,
            _modsProvider.MultipleDependenciesMod
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        EDuke32 eduke32 = new();

        var args = eduke32.GetStartGameArgs(_dukeGame, _dukeCamp, mods, [], true, true, 3);
        var expected = "" +
            $" -g \"enabled_mod.zip\"" +
            $" -mh \"ENABLED1.DEF\"" +
            $" -mh \"ENABLED2.DEF\"" +
            $" -mx \"ENABLED1.CON\"" +
            $" -mx \"ENABLED2.CON\"" +
            $" -g \"mod_incompatible_with_addon.zip\"" +
            $" -g \"incompatible_mod_with_compatible_version.zip\"" +
            $" -g \"dependent_mod.zip\"" +
            $" -g \"dependent_mod_with_compatible_version.zip\"" +
            $" -g \"feature_mod.zip\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Duke3D\\Mods\"" +
            $" -usecwd" +
            $" -cachesize 262144" +
            $" -h \"a\"" +
            $" -j \"D:\\Games\\Duke3D\"" +
            $" -s3" +
            $" -quick" +
            $" -nosetup" +
            $"";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void EDuke32WtTest()
    {
        EDuke32 eduke32 = new();

        var args = eduke32.GetStartGameArgs(_dukeGame, _dukeWtCamp, new Dictionary<AddonId, BaseAddon>(), [], true, true);
        var expected = $"" +
            $" -usecwd" +
            $" -cachesize 262144" +
            $" -h \"a\"" +
            $" -j \"D:\\Games\\DukeWT\"" +
            $" -addon 0" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Ports\\EDuke32\\WTStopgap\"" +
            $" -gamegrp e32wt.grp" +
            $" -mh e32wt.def" +
            $" -quick" +
            $" -nosetup" +
            $"";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void EDuke32VacaTest()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledModWithCons,
            _modsProvider.ModThatRequiresOfficialAddon,
            _modsProvider.ModThatIncompatibleWithAddon
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        EDuke32 eduke32 = new();

        var args = eduke32.GetStartGameArgs(_dukeGame, _dukeVaca, mods, [], true, true);
        var expected = $"" +
            $" -g \"enabled_mod.zip\"" +
            $" -mh \"ENABLED1.DEF\"" +
            $" -mh \"ENABLED2.DEF\"" +
            $" -mx \"ENABLED1.CON\"" +
            $" -mx \"ENABLED2.CON\"" +
            $" -g \"mod_requires_addon.zip\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Duke3D\\Mods\"" +
            $" -usecwd" +
            $" -cachesize 262144" +
            $" -h \"a\"" +
            $" -j \"D:\\Games\\Duke3D\"" +
            $" -j \"D:\\Games\\Duke3D\\Vaca\"" +
            $" -grp VACATION.GRP" +
            $" -quick" +
            $" -nosetup" +
            $"";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void EDuke32TCTest()
    {
        EDuke32 eduke32 = new();

        var args = eduke32.GetStartGameArgs(_dukeGame, _dukeTcForVaca, new Dictionary<AddonId, BaseAddon>(), [], true, true);
        var expected = $"" +
            $" -usecwd" +
            $" -cachesize 262144" +
            $" -h \"TC.DEF\"" +
            $" -mh \"TC1.DEF\"" +
            $" -mh \"TC2.DEF\"" +
            $" -j \"D:\\Games\\Duke3D\"" +
            $" -j \"D:\\Games\\Duke3D\\Vaca\"" +
            $" -grp VACATION.GRP" +
            $" -x \"TC.CON\"" +
            $" -mx \"TC1.CON\"" +
            $" -mx \"TC2.CON\"" +
            $" -g \"{Path.Combine(Directory.GetCurrentDirectory(), "Data", "Duke3D", "Campaigns", "duke_tc.zip")}\"" +
            $" -quick" +
            $" -nosetup" +
            $"";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void RedNukem64Test()
    {
        RedNukem redNukem = new();

        var args = redNukem.GetStartGameArgs(_dukeGame, _duke64Camp, new Dictionary<AddonId, BaseAddon>(), [], true, true);
        var expected = "" +
            " -usecwd" +
            " -d blank.edm" +
            " -h \"a\"" +
            " -j \"D:\\Games\\Duke64\"" +
            " -gamegrp \"rom.z64\"" +
            " -quick" +
            " -nosetup" +
            "";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void RedNukemTest()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledModWithCons,
            _modsProvider.DisabledMod,
            _modsProvider.ModThatRequiresOfficialAddon,
            _modsProvider.ModThatIncompatibleWithAddon,
            _modsProvider.IncompatibleMod,
            _modsProvider.IncompatibleModWithIncompatibleVersion,
            _modsProvider.IncompatibleModWithCompatibleVersion,
            _modsProvider.DependentMod,
            _modsProvider.DependentModWithCompatibleVersion,
            _modsProvider.DependentModWithIncompatibleVersion,
            _modsProvider.ModForAnotherGame,
            _modsProvider.ModThatRequiresFeature,
            _modsProvider.MultipleDependenciesMod
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        RedNukem redNukem = new();

        var args = redNukem.GetStartGameArgs(_dukeGame, _dukeCamp, mods, [], true, true);
        var expected = $"" +
            $" -g \"enabled_mod.zip\"" +
            $" -mh \"ENABLED1.DEF\"" +
            $" -mh \"ENABLED2.DEF\"" +
            $" -mx \"ENABLED1.CON\"" +
            $" -mx \"ENABLED2.CON\"" +
            $" -g \"mod_incompatible_with_addon.zip\"" +
            $" -g \"incompatible_mod_with_compatible_version.zip\"" +
            $" -g \"dependent_mod.zip\"" +
            $" -g \"dependent_mod_with_compatible_version.zip\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Duke3D\\Mods\"" +
            $" -usecwd" +
            " -d blank.edm" +
            $" -h \"a\"" +
            $" -j \"D:\\Games\\Duke3D\"" +
            $" -quick" +
            $" -nosetup" +
            $"";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void RedNukemVacaTest()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledModWithCons,
            _modsProvider.ModThatRequiresOfficialAddon,
            _modsProvider.ModThatIncompatibleWithAddon
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        RedNukem eduke32 = new();

        var args = eduke32.GetStartGameArgs(_dukeGame, _dukeVaca, mods, [], true, true);
        var expected = $"" +
            $" -g \"enabled_mod.zip\"" +
            $" -mh \"ENABLED1.DEF\"" +
            $" -mh \"ENABLED2.DEF\"" +
            $" -mx \"ENABLED1.CON\"" +
            $" -mx \"ENABLED2.CON\"" +
            $" -g \"mod_requires_addon.zip\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Duke3D\\Mods\"" +
            $" -usecwd" +
            " -d blank.edm" +
            $" -h \"a\"" +
            $" -j \"D:\\Games\\Duke3D\"" +
            $" -j \"D:\\Games\\Duke3D\\Vaca\"" +
            $" -g VACATION.GRP" +
            $" -quick" +
            $" -nosetup"
            ;

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void RedNukemTCTest()
    {
        RedNukem eduke32 = new();

        var args = eduke32.GetStartGameArgs(_dukeGame, _dukeTcForVaca, new Dictionary<AddonId, BaseAddon>(), [], true, true);
        var expected = $"" +
            $" -usecwd" +
            " -d blank.edm" +
            $" -h \"TC.DEF\"" +
            $" -mh \"TC1.DEF\"" +
            $" -mh \"TC2.DEF\"" +
            $" -j \"D:\\Games\\Duke3D\"" +
            $" -j \"D:\\Games\\Duke3D\\Vaca\"" +
            $" -g VACATION.GRP" +
            $" -x \"TC.CON\"" +
            $" -mx \"TC1.CON\"" +
            $" -mx \"TC2.CON\"" +
            $" -g \"{Path.Combine(Directory.GetCurrentDirectory(), "Data", "Duke3D", "Campaigns", "duke_tc.zip")}\"" +
            $" -quick" +
            $" -nosetup" +
            $"";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }
}