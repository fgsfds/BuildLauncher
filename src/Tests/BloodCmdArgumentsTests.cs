using Common;
using Common.Enums;
using Common.Enums.Addons;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Tests;

[Collection("Sync")]
public class BloodLineArgumentsTests
{
    private readonly BloodGame _bloodGame;
    private readonly BloodCampaign _bloodCamp;
    private readonly BloodCampaign _bloodCpCamp;
    private readonly BloodCampaign _bloodTc;

    private readonly AutoloadMod _enabledMod;
    private readonly AutoloadMod _incompatibleMod;
    private readonly AutoloadMod _incompatibleModWithIncompatibleVersion;
    private readonly AutoloadMod _incompatibleModWithCompatibleVersion;
    private readonly AutoloadMod _dependantMod;
    private readonly AutoloadMod _dependantModWithIncompatibleVersion;
    private readonly AutoloadMod _dependantModWithCompatibleVersion;
    private readonly AutoloadMod _disabledMod;
    private readonly AutoloadMod _customDudeMod;
    private readonly AutoloadMod _cpMod;

    public BloodLineArgumentsTests()
    {
        _bloodGame = new()
        {
            GameInstallFolder = @"D:\Games\Blood"
        };

        _bloodCamp = new()
        {
            Id = nameof(GameEnum.Blood).ToLower(),
            Type = AddonTypeEnum.Official,
            Title = "Blood",
            GridImage = null,
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
            PreviewImage = null,
            INI = null,
            RFF = null,
            SND = null
        };

        _bloodCpCamp = new()
        {
            Id = nameof(BloodAddonEnum.BloodCP).ToLower(),
            Type = AddonTypeEnum.Official,
            Title = "Cryptic Passage",
            GridImage = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            PathToFile = null,
            DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { nameof(BloodAddonEnum.BloodCP), null } },
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            INI = null,
            RFF = null,
            SND = null
        };

        _bloodTc = new()
        {
            Id = "blood-tc",
            Type = AddonTypeEnum.TC,
            Title = "Blood TC",
            GridImage = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            PathToFile = @"D:\Mods\blood_tc.zip",
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            INI = "TC.INI",
            RFF = "TC.RFF",
            SND = "TC.SND"
        };

        _enabledMod = new()
        {
            Id = "blood-enabledMod",
            Type = AddonTypeEnum.Mod,
            Title = "enabledMod",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.5",
            SupportedGame = new(GameEnum.Blood),
            IncompatibleAddons = null,
            RequiredFeatures = null,
            PathToFile = "enabled_mod.zip",
            DependentAddons = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = ["ENABLED1.DEF", "ENABLED2.DEF"],
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };

        _incompatibleMod = new()
        {
            Id = "blood-incompatibleMod",
            Type = AddonTypeEnum.Mod,
            Title = "incompatibleMod",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.0",
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            PathToFile = null,
            DependentAddons = null,
            IncompatibleAddons = new(StringComparer.OrdinalIgnoreCase) { { "blood-enabledMod", null } },
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };

        _incompatibleModWithCompatibleVersion = new()
        {
            Id = "blood-incompatibleModWithIncompatibleVersion",
            Type = AddonTypeEnum.Mod,
            Title = "incompatibleModWithIncompatibleVersion",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.0",
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            PathToFile = "incompatible_mod_with_compatible_version.zip",
            DependentAddons = null,
            IncompatibleAddons = new(StringComparer.OrdinalIgnoreCase) { { "blood-enabledMod", "<=1.0" } },
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };

        _incompatibleModWithIncompatibleVersion = new()
        {
            Id = "blood-incompatibleModWithCompatibleVersion",
            Type = AddonTypeEnum.Mod,
            Title = "incompatibleModWithCompatibleVersion",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.0",
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            PathToFile = "incompatible_mod_with_incompatible_version.zip",
            DependentAddons = null,
            IncompatibleAddons = new(StringComparer.OrdinalIgnoreCase) { { "blood-enabledMod", ">1.1" } },
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };

        _dependantMod = new()
        {
            Id = "blood-dependantMod",
            Type = AddonTypeEnum.Mod,
            Title = "dependantMod",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.0",
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            PathToFile = "dependant_mod.zip",
            DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { "blood-enabledMod", null } },
            IncompatibleAddons = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };

        _dependantModWithIncompatibleVersion = new()
        {
            Id = "blood-dependantModWithIncompatibleVersion",
            Type = AddonTypeEnum.Mod,
            Title = "dependantModWithIncompatibleVersion",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.0",
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            PathToFile = "dependant_mod_with_incompatible_version.zip",
            DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { "blood-enabledMod", "<=1.0" } },
            IncompatibleAddons = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };

        _dependantModWithCompatibleVersion = new()
        {
            Id = "blood-dependantModWithCompatibleVersion",
            Type = AddonTypeEnum.Mod,
            Title = "dependantModWithCompatibleVersion",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.0",
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            PathToFile = "dependant_mod_with_compatible_version.zip",
            DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { "blood-enabledMod", ">1.1" } },
            IncompatibleAddons = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };

        _disabledMod = new()
        {
            Id = "blood-disabledMod",
            Type = AddonTypeEnum.Mod,
            Title = "disabledMod",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.0",
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            PathToFile = "disabled_mod.zip",
            DependentAddons = null,
            IncompatibleAddons = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = false
        };

        _customDudeMod = new()
        {
            Id = "blood-customDudeMode",
            Type = AddonTypeEnum.Mod,
            Title = "customDudeMode",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.0",
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = [FeatureEnum.CustomDude],
            PathToFile = "custom_dude_mod.zip",
            DependentAddons = null,
            IncompatibleAddons = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };

        _cpMod = new()
        {
            Id = "blood-cpMod",
            Type = AddonTypeEnum.Mod,
            Title = "cpMod",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.0",
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            PathToFile = "cp_mod.zip",
            DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { nameof(BloodAddonEnum.BloodCP), null } },
            IncompatibleAddons = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };
    }

    [Fact]
    public void RazeTest()
    {
        var mods = new List<AutoloadMod>() { 
            _enabledMod, 
            _incompatibleMod,
            _disabledMod, 
            _incompatibleModWithIncompatibleVersion,
            _incompatibleModWithCompatibleVersion,
            _dependantMod,
            _dependantModWithIncompatibleVersion,
            _dependantModWithCompatibleVersion,
            _customDudeMod,
            _cpMod
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        Raze raze = new();

        var args = raze.GetStartGameArgs(_bloodGame, _bloodCamp, mods, true, true);
        var expected = @$" -quick -nosetup -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -file ""incompatible_mod_with_compatible_version.zip"" -file ""dependant_mod.zip"" -file ""dependant_mod_with_compatible_version.zip"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Ports\Raze\Save\blood"" -def ""a""";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', '/');
            expected = expected.Replace('\\', '/');
        }

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
            [GameSearch.Directories]
            Path=D:/Games/Blood

            [FileSearch.Directories]
            Path=D:/Games/Blood
            Path={Directory.GetCurrentDirectory()}/Data/Blood/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void RazeCpTest()
    {
        var mods = new List<AutoloadMod>() { 
            _enabledMod, 
            _incompatibleMod,
            _disabledMod, 
            _incompatibleModWithIncompatibleVersion,
            _incompatibleModWithCompatibleVersion,
            _dependantMod,
            _dependantModWithIncompatibleVersion,
            _dependantModWithCompatibleVersion,
            _customDudeMod,
            _cpMod
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        Raze raze = new();

        var args = raze.GetStartGameArgs(_bloodGame, _bloodCpCamp, mods, true, true);
        var expected = @$" -quick -nosetup -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -file ""incompatible_mod_with_compatible_version.zip"" -file ""dependant_mod.zip"" -file ""dependant_mod_with_compatible_version.zip"" -file ""cp_mod.zip"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Ports\Raze\Save\bloodcp"" -def ""a"" -ini ""CRYPTIC.INI""";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', '/');
            expected = expected.Replace('\\', '/');
        }

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
            [GameSearch.Directories]
            Path=D:/Games/Blood

            [FileSearch.Directories]
            Path=D:/Games/Blood
            Path={Directory.GetCurrentDirectory()}/Data/Blood/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void RazeTCTest()
    {
        Raze raze = new();

        var args = raze.GetStartGameArgs(_bloodGame, _bloodTc, [], true, true);
        var expected = @$" -quick -nosetup -savedir ""{Directory.GetCurrentDirectory()}\Data\Ports\Raze\Save\blood-tc"" -def ""a"" -ini ""TC.INI"" -rff ""TC.RFF"" -snd ""TC.SND"" -file ""{Directory.GetCurrentDirectory()}\Data\Blood\Campaigns\blood_tc.zip""";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', '/');
            expected = expected.Replace('\\', '/');
        }

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
            [GameSearch.Directories]
            Path=D:/Games/Blood

            [FileSearch.Directories]
            Path=D:/Games/Blood
            Path={Directory.GetCurrentDirectory()}/Data/Blood/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void NBloodTest()
    {
        var mods = new List<AutoloadMod>() { 
            _enabledMod, 
            _incompatibleMod,
            _disabledMod, 
            _incompatibleModWithIncompatibleVersion,
            _incompatibleModWithCompatibleVersion,
            _dependantMod,
            _dependantModWithIncompatibleVersion,
            _dependantModWithCompatibleVersion,
            _customDudeMod,
            _cpMod
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        NBlood nblood = new();

        var args = nblood.GetStartGameArgs(_bloodGame, _bloodCamp, mods, true, true);
        var expected = @$" -quick -nosetup -j ""{Directory.GetCurrentDirectory()}\Data\Blood\Mods"" -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -g ""incompatible_mod_with_compatible_version.zip"" -g ""dependant_mod.zip"" -g ""dependant_mod_with_compatible_version.zip"" -g ""custom_dude_mod.zip"" -usecwd -j ""D:\Games\Blood"" -h ""a""";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', '/');
            expected = expected.Replace('\\', '/');
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void NBloodCPTest()
    {
        var mods = new List<AutoloadMod>() { 
            _enabledMod, 
            _incompatibleMod,
            _disabledMod, 
            _incompatibleModWithIncompatibleVersion,
            _incompatibleModWithCompatibleVersion,
            _dependantMod,
            _dependantModWithIncompatibleVersion,
            _dependantModWithCompatibleVersion,
            _customDudeMod,
            _cpMod
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        NBlood nblood = new();

        var args = nblood.GetStartGameArgs(_bloodGame, _bloodCpCamp, mods, true, true);
        var expected = @$" -quick -nosetup -j ""{Directory.GetCurrentDirectory()}\Data\Blood\Mods"" -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -g ""incompatible_mod_with_compatible_version.zip"" -g ""dependant_mod.zip"" -g ""dependant_mod_with_compatible_version.zip"" -g ""custom_dude_mod.zip"" -g ""cp_mod.zip"" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""CRYPTIC.INI""";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', '/');
            expected = expected.Replace('\\', '/');
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void NBloodTCTest()
    {
        NBlood nblood = new();

        var args = nblood.GetStartGameArgs(_bloodGame, _bloodTc, [], true, true);
        var expected = @$" -quick -nosetup -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""TC.INI"" -rff ""TC.RFF"" -snd ""TC.SND"" -g ""{Directory.GetCurrentDirectory()}\Data\Blood\Campaigns\blood_tc.zip""";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', '/');
            expected = expected.Replace('\\', '/');
        }

        Assert.Equal(expected, args);
    }
}