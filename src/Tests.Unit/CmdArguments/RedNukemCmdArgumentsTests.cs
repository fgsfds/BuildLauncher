using Addons.Addons;
using Games.Games;
using Ports.Ports.EDuke32;
using Tests.Unit.Helpers;

namespace Tests.Unit.CmdArguments;

public sealed class RedNukemCmdArgumentsTests
{
    private readonly DukeGame _dukeGame;
    private readonly DukeCampaign _dukeCamp;
    private readonly DukeCampaign _dukeVaca;
    private readonly DukeCampaign _dukeTcForVaca;
    private readonly DukeCampaign _duke64Camp;
    private readonly LooseMap _dukeLooseMap;
    private readonly AutoloadModsTestSetups _dukeMods;

    private readonly NamGame _namGame;
    private readonly DukeCampaign _namCamp;
    private readonly AutoloadModsTestSetups _namMods;

    private readonly RedneckGame _redneckGame;
    private readonly DukeCampaign _redneckCamp;
    private readonly DukeCampaign _redneckAgainCamp;
    private readonly AutoloadModsTestSetups _redneckMods;

    public RedNukemCmdArgumentsTests()
    {
        (_dukeGame, _dukeCamp, _dukeVaca, _dukeTcForVaca, _, _duke64Camp, _, _, _, _dukeLooseMap, _dukeMods) = PortTestSetups.Duke3D();
        (_namGame, _namCamp, _namMods) = PortTestSetups.Nam();
        (_redneckGame, _redneckCamp, _redneckAgainCamp, _, _redneckMods) = PortTestSetups.Redneck();
    }

    [Fact]
    public void Duke64Test()
    {
        RedNukem redNukem = new();

        var args = redNukem.GetStartGameArgs(_dukeGame, _duke64Camp, [], [], true, true);
        var expected = "" +
            " -usecwd" +
            " -d blank.edm" +
            " -h \"a\"" +
            " -j \"D:\\Games\\Duke64\"" +
            " -gamegrp \"rom.z64\"" +
            " -quick" +
            " -nosetup" +
            "";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void DukeTest()
    {
        var mods = _dukeMods.StandardModsWithCons;

        RedNukem redNukem = new();

        var args = redNukem.GetStartGameArgs(_dukeGame, _dukeCamp, mods, [], true, true);
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
            " -h \"a\"" +
            " -j \"D:\\Games\\Duke3D\"" +
            " -quick" +
            " -nosetup" +
            "";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void DukeVacaTest()
    {
        var mods = _dukeMods.AddonModsWithCons;

        RedNukem eduke32 = new();

        var args = eduke32.GetStartGameArgs(_dukeGame, _dukeVaca, mods, [], true, true);
        var expected = $"" +
            $" -g \"enabled_mod.zip\"" +
            $" -mh \"ENABLED1.DEF\"" +
            $" -mh \"ENABLED2.DEF\"" +
            $" -mx \"ENABLED1.CON\"" +
            $" -mx \"ENABLED2.CON\"" +
            $" -g \"mod_requires_addon.zip\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Duke3D\\Mods\"" +
            $" -usecwd" +
            " -d blank.edm" +
            " -h \"a\"" +
            " -j \"D:\\Games\\Duke3D\"" +
            " -j \"D:\\Games\\Duke3D\\Vaca\"" +
            " -g VACATION.GRP" +
            " -quick" +
            " -nosetup"
            ;

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void DukeTCTest()
    {
        RedNukem eduke32 = new();

        var args = eduke32.GetStartGameArgs(_dukeGame, _dukeTcForVaca, [], [], true, true);
        var expected = $"" +
            $" -usecwd" +
            " -d blank.edm" +
            " -h \"TC.DEF\"" +
            " -mh \"TC1.DEF\"" +
            " -mh \"TC2.DEF\"" +
            " -j \"D:\\Games\\Duke3D\"" +
            " -j \"D:\\Games\\Duke3D\\Vaca\"" +
            " -g VACATION.GRP" +
            " -x \"TC.CON\"" +
            " -mx \"TC1.CON\"" +
            " -mx \"TC2.CON\"" +
            $" -g \"{Path.Combine(Directory.GetCurrentDirectory(), "Data", "Duke3D", "Campaigns", "duke_tc.zip")}\"" +
            " -quick" +
            " -nosetup" +
            "";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void DukeLooseMapTest()
    {
        var mods = _dukeMods.StandardModsWithCons;

        RedNukem redNukem = new();

        var args = redNukem.GetStartGameArgs(_dukeGame, _dukeLooseMap, mods, [], true, true);
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
            " -h \"a\"" +
            " -j \"D:\\Games\\Duke3D\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Duke3D\\Maps\"" +
            " -map \"LOOSE.MAP\"" +
            " -quick" +
            " -nosetup" +
            "";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void NamTest()
    {
        var mods = _namMods.MinimalMods;

        RedNukem redNukem = new();

        var args = redNukem.GetStartGameArgs(_namGame, _namCamp, mods, [], true, true);
        var expected = $"" +
            $" -g \"enabled_mod.zip\"" +
            $" -mh \"ENABLED1.DEF\"" +
            $" -mh \"ENABLED2.DEF\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\NAM\\Mods\"" +
            $" -usecwd" +
            " -d blank.edm" +
            " -h \"a\"" +
            " -j \"D:\\Games\\NAM\"" +
            " -nam" +
            " -gamegrp NAM.GRP" +
            " -x GAME.CON" +
            " -quick" +
            " -nosetup" +
            "";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void RedneckTest()
    {
        var mods = _redneckMods.MinimalMods;

        RedNukem redNukem = new();

        var args = redNukem.GetStartGameArgs(_redneckGame, _redneckCamp, mods, [], true, true);
        var expected = $"" +
            $" -g \"enabled_mod.zip\"" +
            $" -mh \"ENABLED1.DEF\"" +
            $" -mh \"ENABLED2.DEF\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Redneck\\Mods\"" +
            $" -usecwd" +
            " -d blank.edm" +
            " -h \"a\"" +
            " -j \"D:\\Games\\Redneck\"" +
            " -quick" +
            " -nosetup" +
            "";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void RedneckAgainTest()
    {
        var mods = _redneckMods.MinimalMods;

        RedNukem redNukem = new();

        var args = redNukem.GetStartGameArgs(_redneckGame, _redneckAgainCamp, mods, [], true, true);
        var expected = $"" +
            $" -g \"enabled_mod.zip\"" +
            $" -mh \"ENABLED1.DEF\"" +
            $" -mh \"ENABLED2.DEF\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Redneck\\Mods\"" +
            $" -usecwd" +
            " -d blank.edm" +
            " -h \"a\"" +
            " -j \"D:\\Games\\Again\"" +
            " -quick" +
            " -nosetup" +
            "";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }
}
