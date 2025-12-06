using Addons.Addons;
using Common.All.Enums;
using Common.All.Enums.Addons;
using Games.Games;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Tests.CmdArguments;

[Collection("Sync")]
public sealed class BloodCmdArgumentsTests
{
    private readonly BloodGame _bloodGame;
    private readonly BloodCampaignEntity _bloodCamp;
    private readonly BloodCampaignEntity _bloodCampWithOptions;
    private readonly BloodCampaignEntity _bloodCpCamp;
    private readonly BloodCampaignEntity _bloodTc;
    private readonly BloodCampaignEntity _bloodTcFolder;
    private readonly BloodCampaignEntity _bloodTcExeOverride;
    private readonly BloodCampaignEntity _bloodTcIncompatibleWithEnabledMod;
    private readonly BloodCampaignEntity _bloodTcIncompatibleWithEverything;

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
            AddonId = new(nameof(GameEnum.Blood).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Blood",
            GridImageHash = null,
            Author = null,
            Description = null,
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
            IsUnpacked = false,
            Executables = null,
            Options = null
        };

        _bloodCampWithOptions = new()
        {
            AddonId = new(nameof(GameEnum.Blood).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Blood",
            GridImageHash = null,
            Author = null,
            Description = null,
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
            IsUnpacked = false,
            Executables = null,
            Options = new() {
                { "option 1", new() { { "OPT.DEF", OptionalParameterTypeEnum.DEF } } },
                { "option 2", new() { { "OPT2.DEF", OptionalParameterTypeEnum.DEF }, { "OPT2_2.DEF", OptionalParameterTypeEnum.DEF } } },
            }
        };

        _bloodCpCamp = new()
        {
            AddonId = new(nameof(BloodAddonEnum.BloodCP).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Cryptic Passage",
            GridImageHash = null,
            Author = null,
            Description = null,
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
            IsUnpacked = false,
            Executables = null,
            Options = null
        };

        _bloodTc = new()
        {
            AddonId = new("blood-tc", null),
            Type = AddonTypeEnum.TC,
            Title = "Blood TC",
            GridImageHash = null,
            Author = null,
            Description = null,
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
            IsUnpacked = false,
            Executables = null,
            Options = null
        };

        _bloodTcFolder = new()
        {
            AddonId = new("blood-tc-folder", null),
            Type = AddonTypeEnum.TC,
            Title = "Blood TC",
            GridImageHash = null,
            Author = null,
            Description = null,
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
            IsUnpacked = true,
            Executables = null,
            Options = null
        };

        _bloodTcExeOverride = new()
        {
            AddonId = new("blood-tc-exe-override", null),
            Type = AddonTypeEnum.TC,
            Title = "Blood TC",
            GridImageHash = null,
            Author = null,
            Description = null,
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
            IsUnpacked = true,
            Executables = new Dictionary<OSEnum, Dictionary<PortEnum, string>>() { { OSEnum.Windows, new Dictionary<PortEnum, string>() { { PortEnum.NBlood, "nblood.exe" } } } },
            Options = null
        };

        _bloodTcIncompatibleWithEnabledMod = new()
        {
            AddonId = new("blood-tc-imcompatible-with-enabled", null),
            Type = AddonTypeEnum.TC,
            Title = "Blood TC",
            GridImageHash = null,
            Author = null,
            Description = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            PathToFile = Path.Combine("D:", "Games", "Blood", "blood_tc_folder", "addon.json"),
            DependentAddons = null,
            IncompatibleAddons = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { { "enabledMod", null } },
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImageHash = null,
            INI = "TC.INI",
            RFF = "TC.RFF",
            SND = "TC.SND",
            IsUnpacked = true,
            Executables = null,
            Options = null
        };

        _bloodTcIncompatibleWithEverything = new()
        {
            AddonId = new("blood-tc-imcompatible-with-everything", null),
            Type = AddonTypeEnum.TC,
            Title = "Blood TC",
            GridImageHash = null,
            Author = null,
            Description = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            PathToFile = Path.Combine("D:", "Games", "Blood", "blood_tc_folder", "addon.json"),
            DependentAddons = null,
            IncompatibleAddons = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { { "*", null } },
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImageHash = null,
            INI = "TC.INI",
            RFF = "TC.RFF",
            SND = "TC.SND",
            IsUnpacked = true,
            Executables = null,
            Options = null
        };

    }

    [Fact]
    public void RazeTest()
    {
        var mods = new List<AutoloadModEntity>() {
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
            _modsProvider.ModThatRequiresFeature,
            _modsProvider.MultipleDependenciesMod
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        Raze raze = new();

        raze.BeforeStart(_bloodGame, _bloodCamp);
        var args = raze.GetStartGameArgs(_bloodGame, _bloodCamp, mods, [], true, true);
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
        var mods = new List<AutoloadModEntity>() {
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
            _modsProvider.ModThatRequiresFeature
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        Raze raze = new();

        raze.BeforeStart(_bloodGame, _bloodCamp);
        var args = raze.GetStartGameArgs(_bloodGame, _bloodCpCamp, mods, [], true, true);
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
        var args = raze.GetStartGameArgs(_bloodGame, _bloodTc, [], [], true, true);
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
        var args = raze.GetStartGameArgs(_bloodGame, _bloodTcFolder, [], [], true, true);
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
        var mods = new List<AutoloadModEntity>() {
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
            _modsProvider.ModThatRequiresFeature,
            _modsProvider.MultipleDependenciesMod
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        NBlood nblood = new();

        var args = nblood.GetStartGameArgs(_bloodGame, _bloodCamp, mods, [], true, true, 2);
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
        var mods = new List<AutoloadModEntity>() {
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
            _modsProvider.ModThatRequiresFeature
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        NBlood nblood = new();

        var args = nblood.GetStartGameArgs(_bloodGame, _bloodCpCamp, mods, [], true, true, 2);
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

        var args = nblood.GetStartGameArgs(_bloodGame, _bloodTc, [], [], true, true, 2);
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

        var args = nblood.GetStartGameArgs(_bloodGame, _bloodTcFolder, [], [], true, true, 2);
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

        var args = nblood.GetStartGameArgs(_bloodGame, _bloodTcExeOverride, [], [], true, true, 2);
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
        var mods = new List<AutoloadModEntity>() {
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
            _modsProvider.ModThatRequiresFeature
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        NotBlood notblood = new();

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodCamp, mods, [], true, true, 2);
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
        var mods = new List<AutoloadModEntity>() {
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
            _modsProvider.ModThatRequiresFeature
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        NotBlood notblood = new();

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodCpCamp, mods, [], true, true, 2);
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

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodTc, [], [], true, true, 2);
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

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodTcFolder, [], [], true, true, 2);
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

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodTcExeOverride, [], [], true, true, 2);
        var expected = @" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""TC.INI"" -rff ""TC.RFF"" -snd ""TC.SND"" -s 2 -quick -nosetup";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void NBloodIncompatibleWithEnabledModTest()
    {
        var mods = new List<AutoloadModEntity>() {
            _modsProvider.EnabledMod,
            _modsProvider.ModThatIncompatibleWithAddon
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        NotBlood notblood = new();

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodTcIncompatibleWithEnabledMod, mods, [], true, true, 2);
        var expected = @$" -g ""mod_incompatible_with_addon.zip"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\Blood\Mods"" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""TC.INI"" -game_dir ""D:\Games\Blood\blood_tc_folder"" -rff ""TC.RFF"" -snd ""TC.SND"" -s 2 -quick -nosetup";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void NBloodIncompatibleWithEverythingTest()
    {
        var mods = new List<AutoloadModEntity>() {
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
            _modsProvider.ModThatRequiresFeature
        }.ToDictionary(x => x.AddonId, x => (BaseAddon)x);

        NotBlood notblood = new();

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodTcIncompatibleWithEverything, mods, [], true, true, 2);
        var expected = @" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""TC.INI"" -game_dir ""D:\Games\Blood\blood_tc_folder"" -rff ""TC.RFF"" -snd ""TC.SND"" -s 2 -quick -nosetup";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void BloodWithOptionsTest()
    {
        NBlood nblood = new();

        var args = nblood.GetStartGameArgs(_bloodGame, _bloodCampWithOptions, [], ["option 2"], true, true, 2);
        var expected = @" -usecwd -j ""D:\Games\Blood"" -h ""a"" -mh ""OPT2.DEF"" -mh ""OPT2_2.DEF"" -s 2 -quick -nosetup";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }
}
