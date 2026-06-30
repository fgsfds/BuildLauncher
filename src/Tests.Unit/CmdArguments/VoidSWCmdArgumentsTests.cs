using Addons.Addons;
using Games.Games;
using Ports.Ports.EDuke32;
using Tests.Unit.Helpers;

namespace Tests.Unit.CmdArguments;

public sealed class VoidSWCmdArgumentsTests
{
    private readonly GenericCampaign _wangCamp;
    private readonly WangGame _wangGame;
    private readonly LooseMap _wangLooseMap;
    private readonly AutoloadModsTestSetups _wangMods;
    private readonly GenericCampaign _wangTdCamp;

    public VoidSWCmdArgumentsTests()
    {
        (_wangGame, _wangCamp, _wangTdCamp, _wangLooseMap, _wangMods) = PortTestSetups.Wang();
    }

    [Fact]
    public void WangTest()
    {
        var mods = _wangMods.StandardMods;

        VoidSW voidSw = new();

        var args = voidSw.GetStartGameArgs(_wangGame, _wangCamp, mods, [], true, true, 3);

        var expected = "" +
                       " -g\"enabled_mod.zip\"" +
                       " -mh\"ENABLED1.DEF\"" +
                       " -mh\"ENABLED2.DEF\"" +
                       " -g\"mod_incompatible_with_addon.zip\"" +
                       " -g\"incompatible_mod_with_compatible_version.zip\"" +
                       " -g\"dependent_mod.zip\"" +
                       " -g\"dependent_mod_with_compatible_version.zip\"" +
                       " -g\"feature_mod.zip\"" +
                       $" -j\"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Wang\\Mods\"" +
                       " -usecwd" +
                       " -j\"D:\\Games\\Wang\"" +
                       " -h\"a\"" +
                       " -addon0" +
                       " -s3" +
                       " -quick" +
                       " -nosetup"
            ;

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void WangTdTest()
    {
        var mods = _wangMods.AddonMods;

        VoidSW voidSw = new();

        var args = voidSw.GetStartGameArgs(_wangGame, _wangTdCamp, mods, [], true, true, 3);

        var expected = "" +
                       " -g\"enabled_mod.zip\"" +
                       " -mh\"ENABLED1.DEF\"" +
                       " -mh\"ENABLED2.DEF\"" +
                       " -g\"mod_requires_addon.zip\"" +
                       $" -j\"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Wang\\Mods\"" +
                       " -usecwd -j\"D:\\Games\\Wang\"" +
                       " -h\"a\"" +
                       " -addon0" +
                       $" -j\"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Wang\\Campaigns\"" +
                       " -g\"TD.zip\"" +
                       " -s3" +
                       " -quick" +
                       " -nosetup"
            ;

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void WangLooseMapTest()
    {
        var mods = _wangMods.StandardMods;

        VoidSW voidSw = new();

        var args = voidSw.GetStartGameArgs(_wangGame, _wangLooseMap, mods, [], true, true, 3);

        var expected = $"" +
                       $" -g\"enabled_mod.zip\"" +
                       $" -mh\"ENABLED1.DEF\"" +
                       $" -mh\"ENABLED2.DEF\"" +
                       $" -g\"mod_incompatible_with_addon.zip\"" +
                       $" -g\"incompatible_mod_with_compatible_version.zip\"" +
                       $" -g\"dependent_mod.zip\"" +
                       $" -g\"dependent_mod_with_compatible_version.zip\"" +
                       $" -g\"feature_mod.zip\"" +
                       $" -j\"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Wang\\Mods\"" +
                       $" -usecwd" +
                       $" -j\"D:\\Games\\Wang\"" +
                       $" -h\"a\"" +
                       $" -j\"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Wang\\Maps\"" +
                       $" -map \"LOOSE.MAP\"" +
                       $" -s3" +
                       $" -quick" +
                       $" -nosetup" +
                       $"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }
}
