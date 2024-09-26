using Common;
using Common.Enums;
using Common.Enums.Addons;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Tests.CmdArguments;

[Collection("Sync")]
public sealed class WangCmdArgumentsTests
{
    private readonly WangGame _wangGame;
    private readonly WangCampaign _wangCamp;
    private readonly WangCampaign _tdCamp;

    private readonly AutoloadModsProvider _modsProvider;

    public WangCmdArgumentsTests()
    {
        _modsProvider = new(GameEnum.ShadowWarrior);

        _wangGame = new()
        {
            GameInstallFolder = Path.Combine("D:", "Games", "Wang"),
        };

        _wangCamp = new()
        {
            Id = nameof(GameEnum.ShadowWarrior).ToLower(),
            Type = AddonTypeEnum.Official,
            Title = "Shadow Warrior",
            GridImage = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.ShadowWarrior),
            RequiredFeatures = null,
            PathToFile = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsUnpacked = false
        };

        _tdCamp = new()
        {
            Id = nameof(WangAddonEnum.TwinDragon).ToLower(),
            Type = AddonTypeEnum.TC,
            Title = "Twin Dragon",
            GridImage = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.ShadowWarrior),
            RequiredFeatures = null,
            PathToFile = Path.Combine("D:", "Games", "Wang", "TD.zip"),
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsUnpacked = false
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

        var args = raze.GetStartGameArgs(_wangGame, _wangCamp, mods, true, true);
        var expected = @$" -quick -nosetup -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -file ""incompatible_mod_with_compatible_version.zip"" -file ""dependant_mod.zip"" -file ""dependant_mod_with_compatible_version.zip"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Ports\Raze\Save\shadowwarrior"" -def ""a""";

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
            Path={Directory.GetCurrentDirectory()}/Data/Wang/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void RazeTdTest()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledMod,
            _modsProvider.ModThatRequiresOfficialAddon,
            _modsProvider.ModThatIncompatibleWithAddon,
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        Raze raze = new();

        var args = raze.GetStartGameArgs(_wangGame, _tdCamp, mods, true, true);
        var expected = @$" -quick -nosetup -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -file ""mod_requires_addon.zip"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Ports\Raze\Save\twindragon"" -def ""a"" -file ""{Directory.GetCurrentDirectory()}\Data\Wang\Campaigns\TD.zip""";

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
            Path={Directory.GetCurrentDirectory()}/Data/Wang/Mods

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

        var args = voidSw.GetStartGameArgs(_wangGame, _wangCamp, mods, true, true, 3);
        var expected = @$" -quick -nosetup -g""enabled_mod.zip"" -mh""ENABLED1.DEF"" -mh""ENABLED2.DEF"" -g""incompatible_mod_with_compatible_version.zip"" -g""dependant_mod.zip"" -g""dependant_mod_with_compatible_version.zip"" -j""{Directory.GetCurrentDirectory()}\Data\Wang\Mods"" -usecwd -j""D:\Games\Wang"" -h""a"" -addon0 -s3";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void VoidSWTdTest()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledMod,
            _modsProvider.ModThatRequiresOfficialAddon,
            _modsProvider.ModThatIncompatibleWithAddon,
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        VoidSW voidSw = new();

        var args = voidSw.GetStartGameArgs(_wangGame, _tdCamp, mods, true, true, 3);
        var expected = @$" -quick -nosetup -g""enabled_mod.zip"" -mh""ENABLED1.DEF"" -mh""ENABLED2.DEF"" -g""mod_requires_addon.zip"" -j""{Directory.GetCurrentDirectory()}\Data\Wang\Mods"" -usecwd -j""D:\Games\Wang"" -h""a"" -addon0 -j""{Directory.GetCurrentDirectory()}\Data\Wang\Campaigns"" -g""TD.zip"" -s3";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }
}