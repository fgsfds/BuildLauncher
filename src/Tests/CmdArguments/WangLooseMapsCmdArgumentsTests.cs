using Addons.Addons;
using Common;
using Common.Enums;
using Common.Interfaces;
using Common.Serializable.Addon;
using Games.Games;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Tests.CmdArguments;

[Collection("Sync")]
public sealed class WangLooseMapsCmdArgumentsTests
{
    private readonly WangGame _wangGame;
    private readonly LooseMap _looseMap;

    private readonly AutoloadModsProvider _modsProvider;

    public WangLooseMapsCmdArgumentsTests()
    {
        _modsProvider = new(GameEnum.ShadowWarrior);

        _wangGame = new()
        {
            GameInstallFolder = Path.Combine("D:", "Games", "Wang"),
        };

        _looseMap = new()
        {
            Id = "loose-map",
            Type = AddonTypeEnum.Map,
            Title = "Loose map",
            GridImageHash = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.ShadowWarrior),
            RequiredFeatures = null,
            PathToFile = Path.Combine("Maps", "LOOSE.MAP"),
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = new MapFileDto() { File = "LOOSE.MAP" },
            PreviewImageHash = null,
            IsFolder = false,
            Executables = null,
            BloodIni = null,
        };
    }

    [Fact]
    public void RazeTest()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledMod,
            _modsProvider.ModThatRequiresOfficialAddon,
            _modsProvider.IncompatibleMod,
            _modsProvider.DisabledMod,
            _modsProvider.IncompatibleModWithIncompatibleVersion,
            _modsProvider.IncompatibleModWithCompatibleVersion,
            _modsProvider.DependentMod,
            _modsProvider.DependentModWithCompatibleVersion,
            _modsProvider.DependentModWithIncompatibleVersion,
            _modsProvider.ModForAnotherGame
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        Raze raze = new();

        raze.BeforeStart(_wangGame, _looseMap);
        var args = raze.GetStartGameArgs(_wangGame, _looseMap, mods, true, true);
        var expected = @$" -quick -nosetup -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -file ""incompatible_mod_with_compatible_version.zip"" -file ""dependent_mod.zip"" -file ""dependent_mod_with_compatible_version.zip"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Saves\Raze\Wang\loose-map"" -def ""a"" -file ""{Directory.GetCurrentDirectory()}\Data\Addons\Wang\Maps"" -map ""LOOSE.MAP""";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
            [GameSearch.Directories]
            Path=D:/Games/Wang

            [FileSearch.Directories]
            Path={Directory.GetCurrentDirectory()}/Data/Addons/Wang/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void VoidSWTest()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledMod,
            _modsProvider.ModThatRequiresOfficialAddon,
            _modsProvider.IncompatibleMod,
            _modsProvider.DisabledMod,
            _modsProvider.IncompatibleModWithIncompatibleVersion,
            _modsProvider.IncompatibleModWithCompatibleVersion,
            _modsProvider.DependentMod,
            _modsProvider.DependentModWithCompatibleVersion,
            _modsProvider.DependentModWithIncompatibleVersion,
            _modsProvider.ModForAnotherGame
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        VoidSW voidSw = new();

        var args = voidSw.GetStartGameArgs(_wangGame, _looseMap, mods, true, true, 3);
        var expected = @$" -quick -nosetup -g""enabled_mod.zip"" -mh""ENABLED1.DEF"" -mh""ENABLED2.DEF"" -g""incompatible_mod_with_compatible_version.zip"" -g""dependent_mod.zip"" -g""dependent_mod_with_compatible_version.zip"" -j""{Directory.GetCurrentDirectory()}\Data\Addons\Wang\Mods"" -usecwd -j""D:\Games\Wang"" -h""a"" -j""{Directory.GetCurrentDirectory()}\Data\Addons\Wang\Maps"" -map ""LOOSE.MAP"" -s3";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }
}