using Addons.Addons;
using Common.Enums;
using Common.Enums.Addons;
using Common.Interfaces;
using Games.Games;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Tests.CmdArguments;

[Collection("Sync")]
public sealed class WW2GICmdArgumentsTests
{
    private readonly WW2GIGame _ww2Game;
    private readonly DukeCampaignEntity _ww2Camp;
    private readonly DukeCampaignEntity _platoonCamp;

    private readonly AutoloadModsProvider _modsProvider;

    public WW2GICmdArgumentsTests()
    {
        _modsProvider = new(GameEnum.Redneck);

        _ww2Game = new()
        {
            GameInstallFolder = Path.Combine("D:", "Games", "WW2GI")
        };

        _ww2Camp = new()
        {
            AddonId = new(nameof(GameEnum.WW2GI).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "World War II GI",
            GridImageHash = null,
            Author = null,
            Description = null,
            SupportedGame = new(GameEnum.WW2GI),
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
            IsFolder = false,
            Executables = null
        };

        _platoonCamp = new()
        {
            AddonId = new(nameof(WW2GIAddonEnum.Platoon).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Platoon Leader",
            GridImageHash = null,
            Author = null,
            Description = null,
            SupportedGame = new(GameEnum.WW2GI),
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
            IsFolder = false,
            Executables = null
        };
    }

    [Fact]
    public void RazeTest()
    {
        var mods = new List<AutoloadModEntity>() {
            _modsProvider.EnabledMod,
            _modsProvider.IncompatibleMod,
        }.ToDictionary(x => x.AddonId, x => (IAddon)x);

        Raze raze = new();

        raze.BeforeStart(_ww2Game, _ww2Camp);
        var args = raze.GetStartGameArgs(_ww2Game, _ww2Camp, mods, true, true);
        var expected = $"" +
            $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\WW2GI\\ww2gi\"" +
            $" -def \"a\" -ww2gi" +
            $" -file WW2GI.GRP" +
            $" -con GAME.CON" +
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
            Path=D:/Games/WW2GI

            [FileSearch.Directories]
            Path={Directory.GetCurrentDirectory()}/Data/Addons/WW2GI/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void RazePlatoonTest()
    {
        var mods = new List<AutoloadModEntity>() {
            _modsProvider.EnabledMod,
            _modsProvider.IncompatibleMod,
        }.ToDictionary(x => x.AddonId, x => (IAddon)x);

        Raze raze = new();

        raze.BeforeStart(_ww2Game, _platoonCamp);
        var args = raze.GetStartGameArgs(_ww2Game, _platoonCamp, mods, true, true);
        var expected = $"" +
            $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\WW2GI\\platoon\"" +
            $" -def \"a\"" +
            $" -ww2gi" +
            $" -file WW2GI.GRP" +
            $" -file PLATOONL.DAT" +
            $" -con PLATOONL.DEF" +
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
            Path=D:/Games/WW2GI

            [FileSearch.Directories]
            Path={Directory.GetCurrentDirectory()}/Data/Addons/WW2GI/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void EDuke32Test()
    {
        var mods = new List<AutoloadModEntity>() {
            _modsProvider.EnabledMod,
            _modsProvider.IncompatibleMod,
        }.ToDictionary(x => x.AddonId, x => (IAddon)x);

        EDuke32 eDuke = new();

        var args = eDuke.GetStartGameArgs(_ww2Game, _ww2Camp, mods, true, true);
        var expected = "" +
            " -usecwd" +
            " -cachesize 262144" +
            " -h \"a\"" +
            " -j \"D:\\Games\\WW2GI\"" +
            " -ww2gi" +
            " -gamegrp WW2GI.GRP" +
            " -x GAME.CON" +
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
    public void EDuke32PlatoonTest()
    {
        var mods = new List<AutoloadModEntity>() {
            _modsProvider.EnabledMod,
            _modsProvider.IncompatibleMod,
        }.ToDictionary(x => x.AddonId, x => (IAddon)x);

        EDuke32 eDuke = new();

        var args = eDuke.GetStartGameArgs(_ww2Game, _platoonCamp, mods, true, true);
        var expected = "" +
            " -usecwd" +
            " -cachesize 262144" +
            " -h \"a\"" +
            " -j \"D:\\Games\\WW2GI\"" +
            " -ww2gi -gamegrp WW2GI.GRP" +
            " -grp PLATOONL.DAT" +
            " -x PLATOONL.DEF" +
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
}