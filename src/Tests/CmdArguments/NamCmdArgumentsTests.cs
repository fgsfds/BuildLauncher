using Addons.Addons;
using Common.All.Enums;
using Games.Games;
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
            AddonId = new(nameof(GameEnum.NAM).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "NAM",
            GridImageHash = null,
            Author = null,
            Description = null,
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
            _modsProvider.EnabledMod,
            _modsProvider.IncompatibleMod,
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        Raze raze = new();

        raze.BeforeStart(_namGame, _namCamp);
        var args = raze.GetStartGameArgs(_namGame, _namCamp, mods, [], true, true);
        var expected = $"" +
            $" -file \"enabled_mod.zip\"" +
            $" -adddef \"ENABLED1.DEF\"" +
            $" -adddef \"ENABLED2.DEF\"" +
            $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\NAM\\nam\"" +
            $" -def \"a\"" +
            $" -nam" +
            $" -file NAM.GRP" +
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
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        EDuke32 eduke32 = new();

        var args = eduke32.GetStartGameArgs(_namGame, _namCamp, mods, [], true, true);
        var expected = $"" +
            $" -g \"enabled_mod.zip\"" +
            $" -mh \"ENABLED1.DEF\"" +
            $" -mh \"ENABLED2.DEF\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\NAM\\Mods\"" +
            $" -usecwd" +
            " -cachesize 262144" +
            $" -h \"a\"" +
            $" -j \"D:\\Games\\NAM\"" +
            $" -nam" +
            $" -gamegrp NAM.GRP" +
            $" -x GAME.CON" +
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
    public void RedNukemTest()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledMod,
            _modsProvider.IncompatibleMod,
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        RedNukem redNukem = new();

        var args = redNukem.GetStartGameArgs(_namGame, _namCamp, mods, [], true, true);
        var expected = $"" +
            $" -g \"enabled_mod.zip\"" +
            $" -mh \"ENABLED1.DEF\"" +
            $" -mh \"ENABLED2.DEF\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\NAM\\Mods\"" +
            $" -usecwd" +
            " -d blank.edm" +
            $" -h \"a\"" +
            $" -j \"D:\\Games\\NAM\"" +
            $" -nam" +
            $" -gamegrp NAM.GRP" +
            $" -x GAME.CON" +
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