using Addons.Addons;
using Common.Enums;
using Common.Interfaces;
using Games.Games;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Tests.CmdArguments;

[Collection("Sync")]
public sealed class SlaveCmdArgumentsTests
{
    private readonly SlaveGame _slaveGame;
    private readonly GenericCampaignEntity _slaveCamp;

    private readonly AutoloadModsProvider _modsProvider;

    public SlaveCmdArgumentsTests()
    {
        _modsProvider = new(GameEnum.Slave);

        _slaveGame = new()
        {
            GameInstallFolder = Path.Combine("D:", "Games", "Slave"),
        };

        _slaveCamp = new()
        {
            AddonId = new(nameof(GameEnum.Slave).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Slave",
            GridImageHash = null,
            PreviewImageHash = null,
            Author = null,
            Description = null,
            SupportedGame = new(GameEnum.Slave),
            RequiredFeatures = null,
            PathToFile = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
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

        raze.BeforeStart(_slaveGame, _slaveCamp);
        var args = raze.GetStartGameArgs(_slaveGame, _slaveCamp, mods, true, true);
        var expected = $"" +
            $" -file \"enabled_mod.zip\"" +
            $" -adddef \"ENABLED1.DEF\"" +
            $" -adddef \"ENABLED2.DEF\"" +
            $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Slave\\slave\"" +
            $" -def \"a\"" +
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
            Path=D:/Games/Slave

            [FileSearch.Directories]
            Path={Directory.GetCurrentDirectory()}/Data/Addons/Slave/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void PCExhumedTest()
    {
        var mods = new List<AutoloadModEntity>() {
            _modsProvider.EnabledMod,
            _modsProvider.IncompatibleMod,
        }.ToDictionary(x => x.AddonId, x => (IAddon)x);

        PCExhumed pcExhumed = new();

        var args = pcExhumed.GetStartGameArgs(_slaveGame, _slaveCamp, mods, true, true);
        var expected = $"" +
            $" -g \"enabled_mod.zip\"" +
            $" -mh \"ENABLED1.DEF\"" +
            $" -mh \"ENABLED2.DEF\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Slave\\Mods\"" +
            $" -usecwd" +
            $" -j \"D:\\Games\\Slave\"" +
            $" -h \"a\"" +
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