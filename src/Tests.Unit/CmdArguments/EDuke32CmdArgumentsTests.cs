using Addons.Addons;
using Core.All.Enums;
using Games.Games;
using Ports.Ports.EDuke32;
using Tests.Unit.Helpers;

namespace Tests.Unit.CmdArguments;

public sealed class EDuke32CmdArgumentsTests
{
    private readonly DukeGame _dukeGame;
    private readonly DukeCampaign _dukeCamp;
    private readonly DukeCampaign _dukeVaca;
    private readonly DukeCampaign _dukeTcForVaca;
    private readonly DukeCampaign _dukeWtCamp;
    private readonly LooseMap _dukeLooseMap;
    private readonly AutoloadModsTestSetups _dukeMods;

    private readonly NamGame _namGame;
    private readonly DukeCampaign _namCamp;
    private readonly AutoloadModsTestSetups _namMods;

    private readonly WW2GIGame _ww2Game;
    private readonly DukeCampaign _ww2Camp;
    private readonly DukeCampaign _ww2PlatoonCamp;
    private readonly AutoloadModsTestSetups _ww2Mods;

    public EDuke32CmdArgumentsTests()
    {
        (_dukeGame, _dukeCamp, _dukeVaca, _dukeTcForVaca, _dukeWtCamp, _, _, _, _, _dukeLooseMap, _dukeMods) = PortTestSetups.Duke3D();
        (_namGame, _namCamp, _namMods) = PortTestSetups.Nam();
        (_ww2Game, _ww2Camp, _ww2PlatoonCamp, _ww2Mods) = PortTestSetups.WW2GI();
    }

    [Fact]
    public void DukeTest()
    {
        var mods = _dukeMods.StandardModsWithCons;

        EDuke32 eduke32 = new();

        var args = eduke32.GetStartGameArgs(_dukeGame, _dukeCamp, mods, [], true, true, 3);
        var expected = "" +
            " -g \"enabled_mod.zip\"" +
            " -mh \"ENABLED1.DEF\"" +
            " -mh \"ENABLED2.DEF\"" +
            " -mx \"ENABLED1.CON\"" +
            " -mx \"ENABLED2.CON\"" +
            " -g \"mod_incompatible_with_addon.zip\"" +
            " -g \"incompatible_mod_with_compatible_version.zip\"" +
            " -g \"dependent_mod.zip\"" +
            " -g \"dependent_mod_with_compatible_version.zip\"" +
            " -g \"feature_mod.zip\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Duke3D\\Mods\"" +
            " -usecwd" +
            " -cachesize 262144" +
            " -h \"a\"" +
            " -j \"D:\\Games\\Duke3D\"" +
            " -s3" +
            " -quick" +
            " -nosetup" +
            "";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void DukeWtTest()
    {
        EDuke32 eduke32 = new();

        var args = eduke32.GetStartGameArgs(_dukeGame, _dukeWtCamp, [], [], true, true);
        var expected = $"" +
            $" -usecwd" +
            $" -cachesize 262144" +
            $" -h \"a\"" +
            $" -j \"D:\\Games\\DukeWT\"" +
            $" -addon 0" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Ports\\EDuke32\\WTStopgap\"" +
            $" -gamegrp e32wt.grp" +
            $" -mh e32wt.def" +
            $" -quick" +
            $" -nosetup" +
            $"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void DukeVacaTest()
    {
        var mods = _dukeMods.AddonModsWithCons;

        EDuke32 eduke32 = new();

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
            $" -cachesize 262144" +
            $" -h \"a\"" +
            $" -j \"D:\\Games\\Duke3D\"" +
            $" -j \"D:\\Games\\Duke3D\\Vaca\"" +
            $" -grp VACATION.GRP" +
            $" -quick" +
            $" -nosetup" +
            $"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void DukeTCTest()
    {
        EDuke32 eduke32 = new();

        var args = eduke32.GetStartGameArgs(_dukeGame, _dukeTcForVaca, [], [], true, true);
        var expected = $"" +
            $" -usecwd" +
            $" -cachesize 262144" +
            $" -h \"TC.DEF\"" +
            $" -mh \"TC1.DEF\"" +
            $" -mh \"TC2.DEF\"" +
            $" -j \"D:\\Games\\Duke3D\"" +
            $" -j \"D:\\Games\\Duke3D\\Vaca\"" +
            $" -grp VACATION.GRP" +
            $" -x \"TC.CON\"" +
            $" -mx \"TC1.CON\"" +
            $" -mx \"TC2.CON\"" +
            $" -g \"{Path.Combine(Directory.GetCurrentDirectory(), "Data", "Duke3D", "Campaigns", "duke_tc.zip")}\"" +
            $" -quick" +
            $" -nosetup" +
            $"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void DukePackedAddonTest()
    {
        EDuke32 eduke32 = new();

        var packedCamp = PortTestSetups.PackedDukeAddonCampaign();
        var zipFilePath = packedCamp.FileInfo!.PathToFile;

        var args = eduke32.GetStartGameArgs(_dukeGame, packedCamp, [], [], true, true);
        var expected = "" +
            " -usecwd" +
            " -cachesize 262144" +
            " -h \"a\"" +
            $" -j \"{_dukeGame.GameInstallFolder}\"" +
            $" -g \"{zipFilePath}\"" +
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

        EDuke32 eduke32 = new();

        var args = eduke32.GetStartGameArgs(_dukeGame, _dukeLooseMap, mods, [], true, true, 3);
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
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Duke3D\\Mods\"" +
            $" -usecwd" +
            $" -cachesize 262144" +
            $" -h \"a\"" +
            $" -j \"D:\\Games\\Duke3D\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Duke3D\\Maps\"" +
            $" -map \"LOOSE.MAP\"" +
            $" -s3" +
            $" -quick" +
            $" -nosetup"
            ;

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void NamTest()
    {
        var mods = _namMods.MinimalMods;

        EDuke32 eduke32 = new();

        var args = eduke32.GetStartGameArgs(_namGame, _namCamp, mods, [], true, true);
        var expected = $"" +
            $" -g \"enabled_mod.zip\"" +
            $" -mh \"ENABLED1.DEF\"" +
            $" -mh \"ENABLED2.DEF\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\NAM\\Mods\"" +
            $" -usecwd" +
            " -cachesize 262144" +
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
    public void WW2GITest()
    {
        var mods = _ww2Mods.MinimalMods;

        EDuke32 eDuke = new();

        var args = eDuke.GetStartGameArgs(_ww2Game, _ww2Camp, mods, [], true, true);
        var expected = "" +
            " -g \"enabled_mod.zip\"" +
            " -mh \"ENABLED1.DEF\"" +
            " -mh \"ENABLED2.DEF\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\WW2GI\\Mods\"" +
            " -usecwd" +
            " -cachesize 262144" +
            " -h \"a\"" +
            " -j \"D:\\Games\\WW2GI\"" +
            " -ww2gi" +
            " -gamegrp WW2GI.GRP" +
            " -x GAME.CON" +
            " -quick" +
            " -nosetup" +
            "";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void WW2GIPlatoonTest()
    {
        var mods = _ww2Mods.MinimalMods;

        EDuke32 eDuke = new();

        var args = eDuke.GetStartGameArgs(_ww2Game, _ww2PlatoonCamp, mods, [], true, true);
        var expected = "" +
            " -g \"enabled_mod.zip\"" +
            " -mh \"ENABLED1.DEF\"" +
            " -mh \"ENABLED2.DEF\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\WW2GI\\Mods\"" +
            " -usecwd" +
            " -cachesize 262144" +
            " -h \"a\"" +
            " -j \"D:\\Games\\WW2GI\"" +
            " -ww2gi -gamegrp WW2GI.GRP" +
            " -grp PLATOONL.DAT" +
            " -x PLATOONL.DEF" +
            " -quick" +
            " -nosetup" +
            "";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void BeforeStart_NullFileInfo_DoesNotThrow()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var game = new DukeGame
            {
                Duke64RomPath = null,
                DukeZHRomPath = null,
                DukeWTInstallPath = null,
                GameInstallFolder = tempDir,
                AddonsPaths = [],
            };

            var camp = new DukeCampaign
            {
                AddonId = new("test-camp", null),
                Type = AddonTypeEnum.Official,
                Title = "Test",
                SupportedGame = new(GameEnum.Duke3D, null, null),
                FileInfo = null,
                GridImageHash = null,
                PreviewImageHash = null,
                Description = null,
                Author = null,
                ReleaseDate = null,
                MainDef = null,
                AdditionalDefs = null,
                MainCon = null,
                AdditionalCons = null,
                RTS = null,
                StartMap = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                RequiredFeatures = null,
                Executables = null,
                Options = null,
            };

            EDuke32 eduke32 = new();
            eduke32.BeforeStart(game, camp);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
