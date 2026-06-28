using Addons.Addons;
using Games.Games;
using Ports.Ports;
using Tests.Unit.Helpers;

namespace Tests.Unit.CmdArguments;

public sealed class BuildGDXCmdArgumentsTests
{
    private readonly DukeGame _dukeGame;
    private readonly DukeCampaign _dukeCamp;
    private readonly BloodGame _bloodGame;
    private readonly BloodCampaign _bloodCamp;
    private readonly WangGame _wangGame;
    private readonly GenericCampaign _wangCamp;
    private readonly SlaveGame _slaveGame;
    private readonly GenericCampaign _slaveCamp;
    private readonly RedneckGame _redneckGame;
    private readonly DukeCampaign _redneckCamp;
    private readonly DukeCampaign _ridesAgainCamp;
    private readonly NamGame _namGame;
    private readonly DukeCampaign _namCamp;
    private readonly WitchavenGame _witchavenGame;
    private readonly GenericCampaign _witchavenCamp;
    private readonly TekWarGame _tekWarGame;
    private readonly GenericCampaign _tekWarCamp;

    public BuildGDXCmdArgumentsTests()
    {
        (_dukeGame, _dukeCamp, _, _, _, _, _, _, _, _, _) = PortTestSetups.Duke3D();
        (_bloodGame, _bloodCamp, _, _, _, _, _, _, _, _, _) = PortTestSetups.Blood();
        (_redneckGame, _redneckCamp, _ridesAgainCamp, _, _) = PortTestSetups.Redneck();
        (_wangGame, _wangCamp, _, _, _) = PortTestSetups.Wang();
        (_slaveGame, _slaveCamp, _) = PortTestSetups.Slave();
        (_namGame, _namCamp, _) = PortTestSetups.Nam();
        (_witchavenGame, _witchavenCamp) = PortTestSetups.Witchaven();
        (_tekWarGame, _tekWarCamp) = PortTestSetups.TekWar();
    }

    [Fact]
    public void DukeTest()
    {
        BuildGDX buildGdx = new();
        var args = buildGdx.GetStartGameArgs(_dukeGame, _dukeCamp, [], [], true, true);
        var expected = $" -jar ..\\..\\BuildGDX.jar -path \"{_dukeGame.GameInstallFolder}\" -game DUKE_NUKEM_3D";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void BloodTest()
    {
        BuildGDX buildGdx = new();
        var args = buildGdx.GetStartGameArgs(_bloodGame, _bloodCamp, [], [], true, true);
        var expected = $" -jar ..\\..\\BuildGDX.jar -path \"{_bloodGame.GameInstallFolder}\" -game BLOOD";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void WangTest()
    {
        BuildGDX buildGdx = new();
        var args = buildGdx.GetStartGameArgs(_wangGame, _wangCamp, [], [], true, true);
        var expected = $" -jar ..\\..\\BuildGDX.jar -path \"{_wangGame.GameInstallFolder}\" -game SHADOW_WARRIOR";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void SlaveTest()
    {
        BuildGDX buildGdx = new();
        var args = buildGdx.GetStartGameArgs(_slaveGame, _slaveCamp, [], [], true, true);
        var expected = $" -jar ..\\..\\BuildGDX.jar -path \"{_slaveGame.GameInstallFolder}\" -game POWERSLAVE";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void RedneckTest()
    {
        BuildGDX buildGdx = new();
        var args = buildGdx.GetStartGameArgs(_redneckGame, _redneckCamp, [], [], true, true);
        var expected = $" -jar ..\\..\\BuildGDX.jar -path \"{_redneckGame.GameInstallFolder}\" -game REDNECK_RAMPAGE";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void RidesAgainTest()
    {
        BuildGDX buildGdx = new();
        var args = buildGdx.GetStartGameArgs(_redneckGame, _ridesAgainCamp, [], [], true, true);
        var expected = $" -jar ..\\..\\BuildGDX.jar -path \"{_redneckGame.AgainInstallPath}\" -game RR_RIDES_AGAIN";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void NamTest()
    {
        BuildGDX buildGdx = new();
        var args = buildGdx.GetStartGameArgs(_namGame, _namCamp, [], [], true, true);
        var expected = $" -jar ..\\..\\BuildGDX.jar -path \"{_namGame.GameInstallFolder}\" -game NAM";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void WitchavenTest()
    {
        BuildGDX buildGdx = new();
        var args = buildGdx.GetStartGameArgs(_witchavenGame, _witchavenCamp, [], [], true, true);
        var expected = $" -jar ..\\..\\BuildGDX.jar -path \"{_witchavenGame.GameInstallFolder}\" -game WITCHAVEN";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void TekWarTest()
    {
        BuildGDX buildGdx = new();
        var args = buildGdx.GetStartGameArgs(_tekWarGame, _tekWarCamp, [], [], true, true);
        var expected = $" -jar ..\\..\\BuildGDX.jar -path \"{_tekWarGame.GameInstallFolder}\" -game TEKWAR";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }
}
