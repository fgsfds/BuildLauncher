using Addons.Addons;
using Core.Client.Interfaces;
using Games.Games;
using Moq;
using Ports.Ports;
using Tests.Unit.Helpers;

namespace Tests.Unit.CmdArguments;

public sealed class ZeroHourCmdArgumentsTests
{
    private readonly DukeGame _dukeGame;
    private readonly DukeCampaign _dukeZhCamp;

    public ZeroHourCmdArgumentsTests()
    {
        (_dukeGame, _, _, _, _, _, _dukeZhCamp, _, _, _, _) = PortTestSetups.Duke3D();
    }

    [Fact]
    public void DukeTest()
    {
        Mock<IConfigProvider> _config = new();
        ZHRecomp redNukem = new(_config.Object);

        var args = redNukem.GetStartGameArgs(_dukeGame, _dukeZhCamp, [], [], true, true);

        Assert.Equal(string.Empty, args);
    }
}
