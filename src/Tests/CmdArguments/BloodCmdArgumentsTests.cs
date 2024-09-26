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
public sealed class BloodCmdArgumentsTests
{
    private readonly BloodGame _bloodGame;
    private readonly BloodCampaign _bloodCamp;
    private readonly BloodCampaign _bloodCpCamp;
    private readonly BloodCampaign _bloodTc;

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
            SND = null,
            IsUnpacked = false
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
            SND = null,
            IsUnpacked = false
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
            PathToFile = Path.Combine("D:", "Games", "Blood", "blood_tc.zip"),
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            INI = "TC.INI",
            RFF = "TC.RFF",
            SND = "TC.SND",
            IsUnpacked = false
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
            _modsProvider.DependantMod,
            _modsProvider.DependantModWithCompatibleVersion,
            _modsProvider.DependantModWithIncompatibleVersion,
            _modsProvider.ModForAnotherGame,
            _modsProvider.ModThatRequiredFeature
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        Raze raze = new();

        var args = raze.GetStartGameArgs(_bloodGame, _bloodCamp, mods, true, true);
        var expected = @$" -quick -nosetup -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -file ""mod_incompatible_with_addon.zip"" -file ""incompatible_mod_with_compatible_version.zip"" -file ""dependant_mod.zip"" -file ""dependant_mod_with_compatible_version.zip"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Ports\Raze\Save\blood"" -def ""a""";

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
            Path={Directory.GetCurrentDirectory()}/Data/Blood/Mods

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
            _modsProvider.DependantMod,
            _modsProvider.DependantModWithCompatibleVersion,
            _modsProvider.DependantModWithIncompatibleVersion,
            _modsProvider.ModForAnotherGame,
            _modsProvider.ModThatRequiredFeature
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        Raze raze = new();

        var args = raze.GetStartGameArgs(_bloodGame, _bloodCpCamp, mods, true, true);
        var expected = @$" -quick -nosetup -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -file ""mod_requires_addon.zip"" -file ""incompatible_mod_with_compatible_version.zip"" -file ""dependant_mod.zip"" -file ""dependant_mod_with_compatible_version.zip"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Ports\Raze\Save\bloodcp"" -def ""a"" -ini ""CRYPTIC.INI""";

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
            Path={Directory.GetCurrentDirectory()}/Data/Blood/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void RazeTCTest()
    {
        Raze raze = new();

        var args = raze.GetStartGameArgs(_bloodGame, _bloodTc, [], true, true);
        var expected = @$" -quick -nosetup -savedir ""{Directory.GetCurrentDirectory()}\Data\Ports\Raze\Save\blood-tc"" -def ""a"" -ini ""TC.INI"" -file ""{Directory.GetCurrentDirectory()}\Data\Blood\Campaigns\blood_tc.zip"" -file ""TC.RFF"" -file ""TC.SND""";

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
            Path={Directory.GetCurrentDirectory()}/Data/Blood/Mods

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
            _modsProvider.DependantMod,
            _modsProvider.DependantModWithCompatibleVersion,
            _modsProvider.DependantModWithIncompatibleVersion,
            _modsProvider.ModForAnotherGame,
            _modsProvider.ModThatRequiredFeature
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        NBlood nblood = new();

        var args = nblood.GetStartGameArgs(_bloodGame, _bloodCamp, mods, true, true, 2);
        var expected = @$" -quick -nosetup -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -g ""mod_incompatible_with_addon.zip"" -g ""incompatible_mod_with_compatible_version.zip"" -g ""dependant_mod.zip"" -g ""dependant_mod_with_compatible_version.zip"" -g ""feature_mod.zip"" -j ""{Directory.GetCurrentDirectory()}\Data\Blood\Mods"" -usecwd -j ""D:\Games\Blood"" -h ""a"" -s 2";

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
            _modsProvider.DependantMod,
            _modsProvider.DependantModWithCompatibleVersion,
            _modsProvider.DependantModWithIncompatibleVersion,
            _modsProvider.ModForAnotherGame,
            _modsProvider.ModThatRequiredFeature
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        NBlood nblood = new();

        var args = nblood.GetStartGameArgs(_bloodGame, _bloodCpCamp, mods, true, true, 2);
        var expected = @$" -quick -nosetup -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -g ""mod_requires_addon.zip"" -g ""incompatible_mod_with_compatible_version.zip"" -g ""dependant_mod.zip"" -g ""dependant_mod_with_compatible_version.zip"" -g ""feature_mod.zip"" -j ""{Directory.GetCurrentDirectory()}\Data\Blood\Mods"" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""CRYPTIC.INI"" -s 2";

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
        var expected = @$" -quick -nosetup -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""TC.INI"" -g ""{Directory.GetCurrentDirectory()}\Data\Blood\Campaigns\blood_tc.zip"" -rff ""TC.RFF"" -snd ""TC.SND"" -s 2";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }
}