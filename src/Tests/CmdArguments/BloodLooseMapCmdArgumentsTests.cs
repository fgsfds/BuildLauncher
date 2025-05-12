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
public sealed class BloodLooseMapCmdArgumentsTests
{
    private readonly BloodGame _bloodGame;
    private readonly LooseMap _looseMap;

    private readonly AutoloadModsProvider _modsProvider;

    public BloodLooseMapCmdArgumentsTests()
    {
        _modsProvider = new(GameEnum.Blood);

        _bloodGame = new()
        {
            GameInstallFolder = Path.Combine("D:", "Games", "Blood"),
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
            SupportedGame = new(GameEnum.Blood),
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
            _modsProvider.ModThatRequiredFeature
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        Raze raze = new();

        raze.BeforeStart(_bloodGame, _looseMap);
        var args = raze.GetStartGameArgs(_bloodGame, _looseMap, mods, true, true);
        var expected = @$" -quick -nosetup -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -file ""mod_incompatible_with_addon.zip"" -file ""incompatible_mod_with_compatible_version.zip"" -file ""dependent_mod.zip"" -file ""dependent_mod_with_compatible_version.zip"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Saves\Raze\Blood\loose-map"" -def ""a"" -ini ""BLOOD.INI"" -file ""{Directory.GetCurrentDirectory()}\Data\Addons\Blood\Maps"" -map ""LOOSE.MAP""";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
            [GameSearch.Directories]
            Path=D:/Games/Blood

            [FileSearch.Directories]
            Path={Directory.GetCurrentDirectory()}/Data/Addons/Blood/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void NBloodTest()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledMod,
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
            _modsProvider.ModThatRequiredFeature
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        NBlood nblood = new();

        var args = nblood.GetStartGameArgs(_bloodGame, _looseMap, mods, true, true, 2);
        var expected = @$" -quick -nosetup -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -g ""mod_incompatible_with_addon.zip"" -g ""incompatible_mod_with_compatible_version.zip"" -g ""dependent_mod.zip"" -g ""dependent_mod_with_compatible_version.zip"" -g ""feature_mod.zip"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\Blood\Mods"" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""BLOOD.INI"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\Blood\Maps"" -map ""LOOSE.MAP"" -s 2";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void NotBloodTest()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledMod,
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
            _modsProvider.ModThatRequiredFeature
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        NotBlood notblood = new();

        var args = notblood.GetStartGameArgs(_bloodGame, _looseMap, mods, true, true, 2);
        var expected = @$" -quick -nosetup -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -g ""mod_incompatible_with_addon.zip"" -g ""incompatible_mod_with_compatible_version.zip"" -g ""dependent_mod.zip"" -g ""dependent_mod_with_compatible_version.zip"" -g ""feature_mod.zip"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\Blood\Mods"" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""BLOOD.INI"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\Blood\Maps"" -map ""LOOSE.MAP"" -s 2";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }
}