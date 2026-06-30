using Addons.Addons;
using Games.Games;
using Ports.Ports.EDuke32;
using Tests.Unit.Helpers;

namespace Tests.Unit.CmdArguments;

public sealed class NotBloodCmdArgumentsTests
{
    private readonly BloodCampaign _bloodCamp;
    private readonly BloodCampaign _bloodCpCamp;
    private readonly BloodGame _bloodGame;
    private readonly LooseMap _bloodLooseMap;
    private readonly AutoloadModsTestSetups _bloodMods;
    private readonly BloodCampaign _bloodTc;
    private readonly BloodCampaign _bloodTcExeOverride;
    private readonly BloodCampaign _bloodTcFolder;
    private readonly BloodCampaign _bloodTcIncompatibleWithEnabledMod;
    private readonly BloodCampaign _bloodTcIncompatibleWithEverything;

    public NotBloodCmdArgumentsTests()
    {
        (_bloodGame, _bloodCamp, _, _bloodCpCamp, _bloodTc, _bloodTcFolder, _bloodTcExeOverride, _bloodTcIncompatibleWithEnabledMod, _bloodTcIncompatibleWithEverything, _bloodLooseMap, _bloodMods) = PortTestSetups.Blood();
    }

    [Fact]
    public void BloodTest()
    {
        var mods = _bloodMods.StandardMods;

        NotBlood notblood = new();

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodCamp, mods, [], true, true, 2);
        var expected = @$" -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -g ""mod_incompatible_with_addon.zip"" -g ""incompatible_mod_with_compatible_version.zip"" -g ""dependent_mod.zip"" -g ""dependent_mod_with_compatible_version.zip"" -g ""feature_mod.zip"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\Blood\Mods"" -usecwd -j ""D:\Games\Blood"" -h ""a"" -s 2 -quick -nosetup";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void BloodCPTest()
    {
        var mods = _bloodMods.StandardMods;

        NotBlood notblood = new();

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodCpCamp, mods, [], true, true, 2);
        var expected = @$" -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -g ""mod_requires_addon.zip"" -g ""incompatible_mod_with_compatible_version.zip"" -g ""dependent_mod.zip"" -g ""dependent_mod_with_compatible_version.zip"" -g ""feature_mod.zip"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\Blood\Mods"" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""CRYPTIC.INI"" -s 2 -quick -nosetup";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void BloodTCTest()
    {
        NotBlood notblood = new();

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodTc, [], [], true, true, 2);
        var expected = @" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""TC.INI"" -g ""D:\Games\Blood\blood_tc.zip"" -rff ""TC.RFF"" -snd ""TC.SND"" -s 2 -quick -nosetup";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void BloodTCFolderTest()
    {
        NotBlood notblood = new();

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodTcFolder, [], [], true, true, 2);
        var expected = @" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""TC.INI"" -game_dir ""D:\Games\Blood\blood_tc_folder"" -rff ""TC.RFF"" -snd ""TC.SND"" -s 2 -quick -nosetup";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void BloodTcExeOverride()
    {
        NotBlood notblood = new();

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodTcExeOverride, [], [], true, true, 2);
        var expected = @" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""TC.INI"" -rff ""TC.RFF"" -snd ""TC.SND"" -s 2 -quick -nosetup";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void BloodIncompatibleWithEnabledModTest()
    {
        var mods = _bloodMods.Enabled;

        NotBlood notblood = new();

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodTcIncompatibleWithEnabledMod, mods, [], true, true, 2);
        var expected = @$" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""TC.INI"" -game_dir ""D:\Games\Blood\blood_tc_folder"" -rff ""TC.RFF"" -snd ""TC.SND"" -s 2 -quick -nosetup";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void BloodIncompatibleWithEverythingTest()
    {
        var mods = _bloodMods.StandardMods;

        NotBlood notblood = new();

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodTcIncompatibleWithEverything, mods, [], true, true, 2);
        var expected = @" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""TC.INI"" -game_dir ""D:\Games\Blood\blood_tc_folder"" -rff ""TC.RFF"" -snd ""TC.SND"" -s 2 -quick -nosetup";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void BloodLooseMapTest()
    {
        var mods = _bloodMods.StandardMods;

        NotBlood notblood = new();

        var args = notblood.GetStartGameArgs(_bloodGame, _bloodLooseMap, mods, [], true, true, 2);
        var expected = @$" -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -g ""mod_incompatible_with_addon.zip"" -g ""incompatible_mod_with_compatible_version.zip"" -g ""dependent_mod.zip"" -g ""dependent_mod_with_compatible_version.zip"" -g ""feature_mod.zip"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\Blood\Mods"" -usecwd -j ""D:\Games\Blood"" -h ""a"" -ini ""BLOOD.INI"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\Blood\Maps"" -map ""LOOSE.MAP"" -s 2 -quick -nosetup";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }
}
