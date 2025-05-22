using Addons.Addons;
using Common;
using Common.Enums;
using Common.Enums.Addons;
using Common.Interfaces;
using Games.Games;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Tests.CmdArguments;

[Collection("Sync")]
public sealed class WangCmdArgumentsTests
{
    private readonly WangGame _wangGame;
    private readonly GenericCampaign _wangCamp;
    private readonly GenericCampaign _tdCamp;

    private readonly AutoloadModsProvider _modsProvider;

    public WangCmdArgumentsTests()
    {
        _modsProvider = new(GameEnum.Wang);

        _wangGame = new()
        {
            GameInstallFolder = Path.Combine("D:", "Games", "Wang"),
        };

        _wangCamp = new()
        {
            Id = nameof(GameEnum.Wang).ToLower(),
            Type = AddonTypeEnum.Official,
            Title = "Shadow Warrior",
            GridImageHash = null,
            PreviewImageHash = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.Wang),
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

        _tdCamp = new()
        {
            Id = nameof(WangAddonEnum.TwinDragon).ToLower(),
            Type = AddonTypeEnum.TC,
            Title = "Twin Dragon",
            GridImageHash = null,
            PreviewImageHash = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.Wang),
            RequiredFeatures = null,
            PathToFile = Path.Combine("D:", "Games", "Wang", "TD.zip"),
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

        raze.BeforeStart(_wangGame, _wangCamp);
        var args = raze.GetStartGameArgs(_wangGame, _wangCamp, mods, true, true);
        var expected = "" +
            " -file \"enabled_mod.zip\"" +
            " -adddef \"ENABLED1.DEF\"" +
            " -adddef \"ENABLED2.DEF\"" +
            " -file \"incompatible_mod_with_compatible_version.zip\"" +
            " -file \"dependent_mod.zip\"" +
            " -file \"dependent_mod_with_compatible_version.zip\"" +
            $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Wang\\shadowwarrior\"" +
            " -def \"a\"" +
            " -quick" +
            " -nosetup" +
            "";

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
    public void RazeTdTest()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledMod,
            _modsProvider.ModThatRequiresOfficialAddon,
            _modsProvider.ModThatIncompatibleWithAddon,
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        Raze raze = new();

        raze.BeforeStart(_wangGame, _tdCamp);
        var args = raze.GetStartGameArgs(_wangGame, _tdCamp, mods, true, true);
        var expected = "" +
            " -file \"enabled_mod.zip\"" +
            " -adddef \"ENABLED1.DEF\"" +
            " -adddef \"ENABLED2.DEF\"" +
            " -file \"mod_requires_addon.zip\"" +
            $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Wang\\twindragon\"" +
            " -def \"a\"" +
            " -file \"D:\\Games\\Wang\\TD.zip\"" +
            " -quick" +
            " -nosetup" +
            "";

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

        var args = voidSw.GetStartGameArgs(_wangGame, _wangCamp, mods, true, true, 3);
        var expected = "" +
            " -g\"enabled_mod.zip\"" +
            " -mh\"ENABLED1.DEF\"" +
            " -mh\"ENABLED2.DEF\"" +
            " -g\"incompatible_mod_with_compatible_version.zip\"" +
            " -g\"dependent_mod.zip\"" +
            " -g\"dependent_mod_with_compatible_version.zip\"" +
            $" -j\"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Wang\\Mods\"" +
            " -usecwd" +
            " -j\"D:\\Games\\Wang\"" +
            " -h\"a\"" +
            " -addon0" +
            " -s3" +
            " -quick" +
            " -nosetup"
            ;

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
        var expected = "" +
            " -g\"enabled_mod.zip\"" +
            " -mh\"ENABLED1.DEF\"" +
            " -mh\"ENABLED2.DEF\"" +
            " -g\"mod_requires_addon.zip\"" +
            $" -j\"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Wang\\Mods\"" +
            " -usecwd -j\"D:\\Games\\Wang\"" +
            " -h\"a\"" +
            " -addon0" +
            $" -j\"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Wang\\Campaigns\"" +
            " -g\"TD.zip\"" +
            " -s3" +
            " -quick" +
            " -nosetup"
            ;

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }
}