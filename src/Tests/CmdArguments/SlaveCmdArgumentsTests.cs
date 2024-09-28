using Common;
using Common.Enums;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Tests.CmdArguments;

[Collection("Sync")]
public sealed class SlaveCmdArgumentsTests
{
    private readonly SlaveGame _slaveGame;
    private readonly SlaveCampaign _slaveCamp;

    private readonly AutoloadModsProvider _modsProvider;

    public SlaveCmdArgumentsTests()
    {
        _modsProvider = new(GameEnum.Exhumed);

        _slaveGame = new()
        {
            GameInstallFolder = Path.Combine("D:", "Games", "Slave"),
        };

        _slaveCamp = new()
        {
            Id = nameof(GameEnum.Exhumed).ToLower(),
            Type = AddonTypeEnum.Official,
            Title = "Slave",
            GridImage = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.Exhumed),
            RequiredFeatures = null,
            PathToFile = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsFolder = false
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

        var args = raze.GetStartGameArgs(_slaveGame, _slaveCamp, mods, true, true);
        var expected = @$" -quick -nosetup -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Saves\Raze\Slave\exhumed"" -def ""a""";
   
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
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledMod,
            _modsProvider.IncompatibleMod,
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        PCExhumed pcExhumed = new();

        var args = pcExhumed.GetStartGameArgs(_slaveGame, _slaveCamp, mods, true, true);
        var expected = @$" -quick -nosetup -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\Slave\Mods"" -usecwd -j ""D:\Games\Slave"" -h ""a""";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }
}