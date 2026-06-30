using Addons.Addons;
using Games.Games;
using Ports.Ports.EDuke32;
using Tests.Unit.Helpers;

namespace Tests.Unit.CmdArguments;

public sealed class PCExhumedCmdArgumentsTests
{
    private readonly GenericCampaign _slaveCamp;
    private readonly SlaveGame _slaveGame;
    private readonly AutoloadModsTestSetups _slaveMods;

    public PCExhumedCmdArgumentsTests()
    {
        (_slaveGame, _slaveCamp, _slaveMods) = PortTestSetups.Slave();
    }

    [Fact]
    public void SlaveTest()
    {
        var mods = _slaveMods.MinimalMods;

        PCExhumed pcExhumed = new();

        var args = pcExhumed.GetStartGameArgs(_slaveGame, _slaveCamp, mods, [], true, true);

        var expected = $"" +
                       $" -g \"enabled_mod.zip\"" +
                       $" -mh \"ENABLED1.DEF\"" +
                       $" -mh \"ENABLED2.DEF\"" +
                       $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Slave\\Mods\"" +
                       $" -usecwd" +
                       $" -j \"D:\\Games\\Slave\"" +
                       $" -h \"a\"" +
                       $" -quick" +
                       $" -nosetup" +
                       $"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }
}
