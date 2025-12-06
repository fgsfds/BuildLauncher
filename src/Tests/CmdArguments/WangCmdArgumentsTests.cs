using Addons.Addons;
using Common.All.Enums;
using Common.All.Enums.Addons;
using Games.Games;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Tests.CmdArguments;

[Collection("Sync")]
public sealed class WangCmdArgumentsTests
{
    private readonly WangGame _wangGame;
    private readonly GenericCampaignEntity _wangCamp;
    private readonly GenericCampaignEntity _tdCamp;

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
            AddonId = new(nameof(GameEnum.Wang).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Shadow Warrior",
            GridImageHash = null,
            PreviewImageHash = null,
            Author = null,
            Description = null,
            SupportedGame = new(GameEnum.Wang),
            RequiredFeatures = null,
            PathToFile = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            IsUnpacked = false,
            Executables = null,
            Options = null
        };

        _tdCamp = new()
        {
            AddonId = new(nameof(WangAddonEnum.TwinDragon).ToLower(), null),
            Type = AddonTypeEnum.TC,
            Title = "Twin Dragon",
            GridImageHash = null,
            PreviewImageHash = null,
            Author = null,
            Description = null,
            SupportedGame = new(GameEnum.Wang),
            RequiredFeatures = null,
            PathToFile = Path.Combine("D:", "Games", "Wang", "TD.zip"),
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            IsUnpacked = false,
            Executables = null,
            Options = null
        };
    }

    [Fact]
    public void RazeTest()
    {
        var mods = new List<AutoloadModEntity>() {
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
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        Raze raze = new();

        raze.BeforeStart(_wangGame, _wangCamp);
        var args = raze.GetStartGameArgs(_wangGame, _wangCamp, mods, [], true, true);
        var expected = "" +
            " -file \"enabled_mod.zip\"" +
            " -adddef \"ENABLED1.DEF\"" +
            " -adddef \"ENABLED2.DEF\"" +
            " -file \"incompatible_mod_with_compatible_version.zip\"" +
            " -file \"dependent_mod.zip\"" +
            " -file \"dependent_mod_with_compatible_version.zip\"" +
            $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Wang\\wang\"" +
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
        var mods = new List<AutoloadModEntity>() {
            _modsProvider.EnabledMod,
            _modsProvider.ModThatRequiresOfficialAddon,
            _modsProvider.ModThatIncompatibleWithAddon,
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        Raze raze = new();

        raze.BeforeStart(_wangGame, _tdCamp);
        var args = raze.GetStartGameArgs(_wangGame, _tdCamp, mods, [], true, true);
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
        var mods = new List<AutoloadModEntity>() {
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
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        VoidSW voidSw = new();

        var args = voidSw.GetStartGameArgs(_wangGame, _wangCamp, mods, [], true, true, 3);
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
        var mods = new List<AutoloadModEntity>() {
            _modsProvider.EnabledMod,
            _modsProvider.ModThatRequiresOfficialAddon,
            _modsProvider.ModThatIncompatibleWithAddon,
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        VoidSW voidSw = new();

        var args = voidSw.GetStartGameArgs(_wangGame, _tdCamp, mods, [], true, true, 3);
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