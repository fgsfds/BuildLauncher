using Common;
using Common.Enums;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Tests.CmdArguments;

[Collection("Sync")]
public sealed class NamCmdArgumentsTests
{
    private readonly NamGame _namGame;
    private readonly DukeCampaign _namCamp;

    private readonly AutoloadModsProvider _modsProvider;

    public NamCmdArgumentsTests()
    {
        _modsProvider = new(GameEnum.NAM);

        _namGame = new()
        {
            GameInstallFolder = Path.Combine("D:", "Games", "NAM")
        };

        _namCamp = new()
        {
            Id = nameof(GameEnum.NAM).ToLower(),
            Type = AddonTypeEnum.Official,
            Title = "NAM",
            GridImage = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.NAM),
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

        var args = raze.GetStartGameArgs(_namGame, _namCamp, mods, true, true);
        var expected = @$" -quick -nosetup -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Saves\Raze\NAM\nam"" -def ""a"" -nam -file NAM.GRP -con GAME.CON";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
            [GameSearch.Directories]
            Path=D:/Games/NAM

            [FileSearch.Directories]
            Path={Directory.GetCurrentDirectory()}/Data/Addons/NAM/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void EDuke32Test()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledMod,
            _modsProvider.IncompatibleMod,
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        RedNukem redNukem = new();

        var args = redNukem.GetStartGameArgs(_namGame, _namCamp, mods, true, true);
        var expected = @$" -quick -nosetup -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\NAM\Mods"" -usecwd -h ""a"" -j ""D:\Games\NAM"" -nam -gamegrp NAM.GRP -x GAME.CON";

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
            _modsProvider.EnabledMod,
            _modsProvider.IncompatibleMod,
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        RedNukem redNukem = new();

        var args = redNukem.GetStartGameArgs(_namGame, _namCamp, mods, true, true);
        var expected = @$" -quick -nosetup -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\NAM\Mods"" -usecwd -h ""a"" -j ""D:\Games\NAM"" -nam -gamegrp NAM.GRP -x GAME.CON";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }
}