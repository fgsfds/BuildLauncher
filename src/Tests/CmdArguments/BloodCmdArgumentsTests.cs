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
public sealed class BloodCmdArgumentsTests
{
    private readonly BloodGame _bloodGame;
    private readonly BloodCampaign _bloodCamp;
    private readonly BloodCampaign _bloodCpCamp;
    private readonly BloodCampaign _bloodTc;
    private readonly BloodCampaign _bloodTcFolder;
    private readonly BloodCampaign _bloodTcExeOverride;

    private readonly AutoloadModsProvider _modsProvider;

    public BloodCmdArgumentsTests()
    {
        _modsProvider = new(GameEnum.Blood);

        _bloodGame = new()
        {
            GameInstallFolder = Path.Combine("D:", "Games", "Blood"),
        };

        _bloodCamp = new()
        {
            Id = nameof(GameEnum.Blood).ToLower(),
            Type = AddonTypeEnum.Official,
            Title = "Blood",
            GridImageHash = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            PathToFile = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImageHash = null,
            INI = null,
            RFF = null,
            SND = null,
            IsFolder = false,
            Executables = null
        };

        _bloodCpCamp = new()
        {
            Id = nameof(BloodAddonEnum.BloodCP).ToLower(),
            Type = AddonTypeEnum.Official,
            Title = "Cryptic Passage",
            GridImageHash = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            PathToFile = null,
            DependentAddons = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { { nameof(BloodAddonEnum.BloodCP), null } },
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImageHash = null,
            INI = null,
            RFF = null,
            SND = null,
            IsFolder = false,
            Executables = null
        };

        _bloodTc = new()
        {
            Id = "blood-tc",
            Type = AddonTypeEnum.TC,
            Title = "Blood TC",
            GridImageHash = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            PathToFile = Path.Combine("D:", "Games", "Blood", "blood_tc.zip"),
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImageHash = null,
            INI = "TC.INI",
            RFF = "TC.RFF",
            SND = "TC.SND",
            IsFolder = false,
            Executables = null
        };

        _bloodTcFolder = new()
        {
            Id = "blood-tc-folder",
            Type = AddonTypeEnum.TC,
            Title = "Blood TC",
            GridImageHash = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            PathToFile = Path.Combine("D:", "Games", "Blood", "blood_tc_folder", "addon.json"),
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImageHash = null,
            INI = "TC.INI",
            RFF = "TC.RFF",
            SND = "TC.SND",
            IsFolder = true,
            Executables = null
        };

        _bloodTcExeOverride = new()
        {
            Id = "blood-tc-exe-override",
            Type = AddonTypeEnum.TC,
            Title = "Blood TC",
            GridImageHash = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            PathToFile = Path.Combine("D:", "Games", "Blood", "blood_tc_folder", "addon.json"),
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImageHash = null,
            INI = "TC.INI",
            RFF = "TC.RFF",
            SND = "TC.SND",
            IsFolder = true,
            Executables = new Dictionary<OSEnum, string>() { { OSEnum.Windows, "nblood.exe" } }
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
            _modsProvider.ModThatRequiredFeature,
            _modsProvider.MultipleDependenciesMod
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        Raze raze = new();

        raze.BeforeStart(_bloodGame, _bloodCamp);
        var args = raze.GetStartGameArgs(_bloodGame, _bloodCamp, mods, true, true);
        var expected = @$" -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -file ""mod_incompatible_with_addon.zip"" -file ""incompatible_mod_with_compatible_version.zip"" -file ""dependent_mod.zip"" -file ""dependent_mod_with_compatible_version.zip"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Saves\Raze\Blood\blood"" -def ""a"" -quick -nosetup";

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
    public void RazeCpTest()
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

        raze.BeforeStart(_bloodGame, _bloodCamp);
        var args = raze.GetStartGameArgs(_bloodGame, _bloodCpCamp, mods, true, true);
        var expected = @$" -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -file ""mod_requires_addon.zip"" -file ""incompatible_mod_with_compatible_version.zip"" -file ""dependent_mod.zip"" -file ""dependent_mod_with_compatible_version.zip"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Saves\Raze\Blood\bloodcp"" -def ""a"" -ini ""CRYPTIC.INI"" -quick -nosetup";

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
    public void RazeTCTest()
    {
        Raze raze = new();

        raze.BeforeStart(_bloodGame, _bloodTc);
        var args = raze.GetStartGameArgs(_bloodGame, _bloodTc, [], true, true);
        var expected = @$" -savedir ""{Directory.GetCurrentDirectory()}\Data\Saves\Raze\Blood\blood-tc"" -def ""a"" -ini ""TC.INI"" -file ""D:\Games\Blood\blood_tc.zip"" -file ""TC.RFF"" -file ""TC.SND"" -quick -nosetup";

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
    public void RazeTCFolderTest()
    {
        Raze raze = new();

        raze.BeforeStart(_bloodGame, _bloodTcFolder);
        var args = raze.GetStartGameArgs(_bloodGame, _bloodTcFolder, [], true, true);
        var expected = @$" -savedir ""{Directory.GetCurrentDirectory()}\Data\Saves\Raze\Blood\blood-tc-folder"" -def ""a"" -ini ""TC.INI"" -file ""D:\Games\Blood\blood_tc_folder"" -file ""TC.RFF"" -file ""TC.SND"" -quick -nosetup";

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
            Path=D:/Games/Blood/blood_tc_folder

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
            _modsProvider.ModThatRequiredFeature,
            _modsProvider.MultipleDependenciesMod
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        NBlood nblood = new();

        var args = nblood.GetStartGameArgs(_bloodGame, _bloodCamp, mods, true, true, 2);
        var expected = @$" -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -g ""mod_incompatible_with_addon.zip"" -g ""incompatible_mod_with_compatible_version.zip"" -g ""dependent_mod.zip"" -g ""dependent_mod_with_compatible_version.zip"" -g ""feature_mod.zip"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\Blood\Mods"" -usecwd -j ""D:\Games\Blood"" -h ""a"" -s 2 -quick -nosetup";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void NBloodCPTest()
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

        var args = nblood.GetStartGameArgs(_bloodGame, _bloodCpCamp, mods, true, true, 2);
        var expected = @$" -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -g ""mod_requires_addon.zip"" -g ""incompatible_mod_with_compatible_version.zip"" -g ""dependent_mod.zip"" -g ""dependent_mod_with_compatible_version.zip"" -g ""feature_mod.zip"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\Blood\Mods"" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""CRYPTIC.INI"" -s 2 -quick -nosetup";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void NBloodTCTest()
    {
        NBlood nblood = new();

        var args = nblood.GetStartGameArgs(_bloodGame, _bloodTc, [], true, true, 2);
        var expected = @" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""TC.INI"" -g ""D:\Games\Blood\blood_tc.zip"" -rff ""TC.RFF"" -snd ""TC.SND"" -s 2 -quick -nosetup";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void NBloodTCFolderTest()
    {
        NBlood nblood = new();

        var args = nblood.GetStartGameArgs(_bloodGame, _bloodTcFolder, [], true, true, 2);
        var expected = @" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""TC.INI"" -game_dir ""D:\Games\Blood\blood_tc_folder"" -rff ""TC.RFF"" -snd ""TC.SND"" -s 2 -quick -nosetup";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void NBloodTcExeOverride()
    {
        NBlood nblood = new();

        var args = nblood.GetStartGameArgs(_bloodGame, _bloodTcExeOverride, [], true, true, 2);
        var expected = @" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""TC.INI"" -rff ""TC.RFF"" -snd ""TC.SND"" -s 2 -quick -nosetup";

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

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodCamp, mods, true, true, 2);
        var expected = @$" -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -g ""mod_incompatible_with_addon.zip"" -g ""incompatible_mod_with_compatible_version.zip"" -g ""dependent_mod.zip"" -g ""dependent_mod_with_compatible_version.zip"" -g ""feature_mod.zip"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\Blood\Mods"" -usecwd -j ""D:\Games\Blood"" -h ""a"" -s 2 -quick -nosetup";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void NotBloodCPTest()
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

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodCpCamp, mods, true, true, 2);
        var expected = @$" -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -g ""mod_requires_addon.zip"" -g ""incompatible_mod_with_compatible_version.zip"" -g ""dependent_mod.zip"" -g ""dependent_mod_with_compatible_version.zip"" -g ""feature_mod.zip"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\Blood\Mods"" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""CRYPTIC.INI"" -s 2 -quick -nosetup";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void NotBloodTCTest()
    {
        NotBlood notblood = new();

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodTc, [], true, true, 2);
        var expected = @" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""TC.INI"" -g ""D:\Games\Blood\blood_tc.zip"" -rff ""TC.RFF"" -snd ""TC.SND"" -s 2 -quick -nosetup";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void NotBloodTCFolderTest()
    {
        NotBlood notblood = new();

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodTcFolder, [], true, true, 2);
        var expected = @" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""TC.INI"" -game_dir ""D:\Games\Blood\blood_tc_folder"" -rff ""TC.RFF"" -snd ""TC.SND"" -s 2 -quick -nosetup";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void NotBloodTcExeOverride()
    {
        NotBlood notblood = new();

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodTcExeOverride, [], true, true, 2);
        var expected = @" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""TC.INI"" -rff ""TC.RFF"" -snd ""TC.SND"" -s 2 -quick -nosetup";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }
}