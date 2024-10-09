using Common;
using Common.Enums;
using Common.Enums.Addons;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
using Mods.Serializable.Addon;
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
            GridImage = null,
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
            PreviewImage = null,
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
            _modsProvider.DependantMod,
            _modsProvider.DependantModWithCompatibleVersion,
            _modsProvider.DependantModWithIncompatibleVersion,
            _modsProvider.ModForAnotherGame
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        Raze raze = new();

        var args = raze.GetStartGameArgs(_wangGame, _looseMap, mods, true, true);
        var expected = @$" -quick -nosetup -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -file ""incompatible_mod_with_compatible_version.zip"" -file ""dependant_mod.zip"" -file ""dependant_mod_with_compatible_version.zip"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Saves\Raze\Wang\loose-map"" -def ""a"" -file ""{Directory.GetCurrentDirectory()}\Data\Addons\Wang\Maps"" -map ""LOOSE.MAP""";

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
            _modsProvider.DependantMod,
            _modsProvider.DependantModWithCompatibleVersion,
            _modsProvider.DependantModWithIncompatibleVersion,
            _modsProvider.ModForAnotherGame
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        VoidSW voidSw = new();

        var args = voidSw.GetStartGameArgs(_wangGame, _looseMap, mods, true, true, 3);
        var expected = @$" -quick -nosetup -g""enabled_mod.zip"" -mh""ENABLED1.DEF"" -mh""ENABLED2.DEF"" -g""incompatible_mod_with_compatible_version.zip"" -g""dependant_mod.zip"" -g""dependant_mod_with_compatible_version.zip"" -j""{Directory.GetCurrentDirectory()}\Data\Addons\Wang\Mods"" -usecwd -j""D:\Games\Wang"" -h""a"" -j""{Directory.GetCurrentDirectory()}\Data\Addons\Wang\Maps"" -map ""LOOSE.MAP"" -s3";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }
}