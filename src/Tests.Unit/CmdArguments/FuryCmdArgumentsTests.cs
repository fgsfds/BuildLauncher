using Addons.Addons;
using Core.Client.Config;
using Games.Games;
using Ports.Ports.EDuke32;
using Tests.Unit.Helpers;

namespace Tests.Unit.CmdArguments;

public sealed class FuryCmdArgumentsTests
{
    private readonly DukeCampaign _camp;
    private readonly FuryGame _game;
    private readonly AutoloadModsTestSetups _mods;

    public FuryCmdArgumentsTests()
    {
        (_game, _camp, _mods) = PortTestSetups.Fury();
    }

    [Fact]
    public void FuryTest()
    {
        var mods = _mods.StandardModsWithCons;

        Fury fury = new(new ConfigProviderFake());

        var args = fury.GetStartGameArgs(_game, _camp, mods, [], true, true, 3);

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
                       $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Fury\\Mods\"" +
                       $" -s3" +
                       $" -quick" +
                       $" -nosetup" +
                       $"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }
}
