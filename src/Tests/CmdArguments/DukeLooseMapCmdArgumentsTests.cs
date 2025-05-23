using Addons.Addons;
using Common.Enums;
using Common.Enums.Versions;
using Common.Interfaces;
using Common.Serializable.Addon;
using Games.Games;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Tests.CmdArguments;

[Collection("Sync")]
public sealed class DukeLooseMapCmdArgumentsTests
{
    private readonly DukeGame _dukeGame;
    private readonly LooseMapEntity _dukeLooseMap;

    private readonly AutoloadModsProvider _modsProvider;

    public DukeLooseMapCmdArgumentsTests()
    {
        _modsProvider = new(GameEnum.Duke3D);

        _dukeGame = new()
        {
            GameInstallFolder = Path.Combine("D:", "Games", "Duke3D"),
            Duke64RomPath = null,
            DukeWTInstallPath = null,
        };

        _dukeLooseMap = new()
        {
            AddonId = new("loose-map", null),
            Type = AddonTypeEnum.Map,
            Title = "Loose map",
            GridImageHash = null,
            Author = null,
            Description = null,
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            RequiredFeatures = null,
            PathToFile = Path.Combine("Maps", "LOOSE.MAP"),
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = new MapFileJsonModel() { File = "LOOSE.MAP" },
            PreviewImageHash = null,
            IsFolder = false,
            Executables = null,
            BloodIni = null,
        };
    }

    [Fact]
    public void RazeTest()
    {
        var mods = new List<AutoloadModEntity>() {
            _modsProvider.EnabledModWithCons,
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
        }.ToDictionary(x => x.AddonId, x => (IAddon)x);

        Raze raze = new();

        raze.BeforeStart(_dukeGame, _dukeLooseMap);
        var args = raze.GetStartGameArgs(_dukeGame, _dukeLooseMap, mods, true, true);
        var expected = $"" +
            $" -file \"enabled_mod.zip\"" +
            $" -adddef \"ENABLED1.DEF\"" +
            $" -adddef \"ENABLED2.DEF\"" +
            $" -addcon \"ENABLED1.CON\"" +
            $" -addcon \"ENABLED2.CON\"" +
            $" -file \"mod_incompatible_with_addon.zip\"" +
            $" -file \"incompatible_mod_with_compatible_version.zip\"" +
            $" -file \"dependent_mod.zip\"" +
            $" -file \"dependent_mod_with_compatible_version.zip\"" +
            $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Duke3D\\loose-map\"" +
            $" -def \"a\"" +
            $" -file \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Duke3D\\Maps\"" +
            $" -map \"LOOSE.MAP\"" +
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
            Path=D:/Games/Duke3D

            [FileSearch.Directories]
            Path={Directory.GetCurrentDirectory()}/Data/Addons/Duke3D/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void EDuke32Test()
    {
        var mods = new List<AutoloadModEntity>() {
            _modsProvider.EnabledModWithCons,
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
        }.ToDictionary(x => x.AddonId, x => (IAddon)x);

        EDuke32 eduke32 = new();

        var args = eduke32.GetStartGameArgs(_dukeGame, _dukeLooseMap, mods, true, true, 3);
        var expected = $"" +
            $" -g \"enabled_mod.zip\"" +
            $" -mh \"ENABLED1.DEF\"" +
            $" -mh \"ENABLED2.DEF\"" +
            $" -mx \"ENABLED1.CON\"" +
            $" -mx \"ENABLED2.CON\"" +
            $" -g \"mod_incompatible_with_addon.zip\"" +
            $" -g \"incompatible_mod_with_compatible_version.zip\"" +
            $" -g \"dependent_mod.zip\"" +
            $" -g \"dependent_mod_with_compatible_version.zip\"" +
            $" -g \"feature_mod.zip\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Duke3D\\Mods\"" +
            $" -usecwd" +
            $" -cachesize 262144" +
            $" -h \"a\"" +
            $" -j \"D:\\Games\\Duke3D\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Duke3D\\Maps\"" +
            $" -map \"LOOSE.MAP\"" +
            $" -s3" +
            $" -quick" +
            $" -nosetup"
            ;

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
        var mods = new List<AutoloadModEntity>() {
            _modsProvider.EnabledModWithCons,
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
        }.ToDictionary(x => x.AddonId, x => (IAddon)x);

        RedNukem redNukem = new();

        var args = redNukem.GetStartGameArgs(_dukeGame, _dukeLooseMap, mods, true, true);
        var expected = $"" +
            $" -g \"enabled_mod.zip\"" +
            $" -mh \"ENABLED1.DEF\"" +
            $" -mh \"ENABLED2.DEF\"" +
            $" -mx \"ENABLED1.CON\"" +
            $" -mx \"ENABLED2.CON\"" +
            $" -g \"mod_incompatible_with_addon.zip\"" +
            $" -g \"incompatible_mod_with_compatible_version.zip\"" +
            $" -g \"dependent_mod.zip\"" +
            $" -g \"dependent_mod_with_compatible_version.zip\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Duke3D\\Mods\"" +
            $" -usecwd" +
            " -d blank.edm" +
            $" -h \"a\"" +
            $" -j \"D:\\Games\\Duke3D\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Duke3D\\Maps\"" +
            $" -map \"LOOSE.MAP\"" +
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