using Addons.Addons;
using Common;
using Common.Enums;
using Common.Interfaces;
using Games.Games;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Tests.CmdArguments;

[Collection("Sync")]
public sealed class RedneckCmdArgumentsTests
{
    private readonly RedneckGame _redneckGame;
    private readonly DukeCampaign _redneckCamp;
    private readonly DukeCampaign _againCamp;

    private readonly AutoloadModsProvider _modsProvider;

    public RedneckCmdArgumentsTests()
    {
        _modsProvider = new(GameEnum.Redneck);

        _redneckGame = new()
        {
            GameInstallFolder = Path.Combine("D:", "Games", "Redneck"),
            AgainInstallPath = Path.Combine("D:", "Games", "Again"),
        };

        _redneckCamp = new()
        {
            Id = nameof(GameEnum.Redneck).ToLower(),
            Type = AddonTypeEnum.Official,
            Title = "Redneck Rampage",
            GridImage = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.Redneck),
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
            PreviewImage = null,
            IsFolder = false,
            Executables = null
        };

        _againCamp = new()
        {
            Id = nameof(GameEnum.RidesAgain).ToLower(),
            Type = AddonTypeEnum.Official,
            Title = "Rides Again",
            GridImage = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.RidesAgain),
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
            PreviewImage = null,
            IsFolder = false,
            Executables = null
        };
    }

    [Fact]
    public void RazeTest()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledMod,
            _modsProvider.IncompatibleMod,
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        Raze raze = new();

        raze.BeforeStart(_redneckGame, _redneckCamp);
        var args = raze.GetStartGameArgs(_redneckGame, _redneckCamp, mods, true, true);
        var expected = @$" -quick -nosetup -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Saves\Raze\Redneck\redneck"" -def ""a""";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
            [GameSearch.Directories]
            Path=D:/Games/Redneck

            [FileSearch.Directories]
            Path={Directory.GetCurrentDirectory()}/Data/Addons/Redneck/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void RazeAgainTest()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledMod,
            _modsProvider.IncompatibleMod,
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        Raze raze = new();

        raze.BeforeStart(_redneckGame, _againCamp);
        var args = raze.GetStartGameArgs(_redneckGame, _againCamp, mods, true, true);
        var expected = @$" -quick -nosetup -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Saves\Raze\Redneck\ridesagain"" -def ""a""";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
            [GameSearch.Directories]
            Path=D:/Games/Again

            [FileSearch.Directories]
            Path={Directory.GetCurrentDirectory()}/Data/Addons/Redneck/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void RedNukemTest()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledMod,
            _modsProvider.IncompatibleMod,
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        RedNukem redNukem = new();

        var args = redNukem.GetStartGameArgs(_redneckGame, _redneckCamp, mods, true, true);
        var expected = @$" -quick -nosetup -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\Redneck\Mods"" -usecwd -h ""a"" -j ""D:\Games\Redneck""";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void RedNukemAgainTest()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledMod,
            _modsProvider.IncompatibleMod,
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        RedNukem redNukem = new();

        var args = redNukem.GetStartGameArgs(_redneckGame, _againCamp, mods, true, true);
        var expected = @$" -quick -nosetup -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\Redneck\Mods"" -usecwd -h ""a"" -j ""D:\Games\Again""";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }
}