using Addons.Addons;
using Games.Games;
using Ports.Ports;
using Tests.Unit.Helpers;

namespace Tests.Unit.CmdArguments;

public sealed class RazeCmdArgumentsTests
{
    private readonly BloodCampaign _bloodCamp;
    private readonly BloodCampaign _bloodCpCamp;
    private readonly BloodGame _bloodGame;
    private readonly LooseMap _bloodLooseMap;
    private readonly AutoloadModsTestSetups _bloodMods;
    private readonly BloodCampaign _bloodTc;
    private readonly BloodCampaign _bloodTcFolder;
    private readonly DukeCampaign _dukeCamp;

    private readonly DukeGame _dukeGame;
    private readonly LooseMap _dukeLooseMap;
    private readonly AutoloadModsTestSetups _dukeMods;
    private readonly DukeCampaign _dukeTcForVaca;
    private readonly DukeCampaign _dukeVaca;
    private readonly DukeCampaign _dukeWtCamp;
    private readonly DukeCampaign _namCamp;

    private readonly NamGame _namGame;
    private readonly AutoloadModsTestSetups _namMods;
    private readonly DukeCampaign _redneckAgainCamp;
    private readonly DukeCampaign _redneckCamp;

    private readonly RedneckGame _redneckGame;
    private readonly AutoloadModsTestSetups _redneckMods;
    private readonly GenericCampaign _slaveCamp;

    private readonly SlaveGame _slaveGame;
    private readonly AutoloadModsTestSetups _slaveMods;
    private readonly GenericCampaign _wangCamp;

    private readonly WangGame _wangGame;
    private readonly LooseMap _wangLooseMap;
    private readonly AutoloadModsTestSetups _wangMods;
    private readonly GenericCampaign _wangTdCamp;
    private readonly DukeCampaign _ww2Camp;

    private readonly WW2GIGame _ww2Game;
    private readonly AutoloadModsTestSetups _ww2Mods;
    private readonly DukeCampaign _ww2PlatoonCamp;

    public RazeCmdArgumentsTests()
    {
        (_bloodGame, _bloodCamp, _, _bloodCpCamp, _bloodTc, _bloodTcFolder, _, _, _, _bloodLooseMap, _bloodMods) = PortTestSetups.Blood();
        (_dukeGame, _dukeCamp, _dukeVaca, _dukeTcForVaca, _dukeWtCamp, _, _, _, _, _dukeLooseMap, _dukeMods) = PortTestSetups.Duke3D();
        (_namGame, _namCamp, _namMods) = PortTestSetups.Nam();
        (_redneckGame, _redneckCamp, _redneckAgainCamp, _, _redneckMods) = PortTestSetups.Redneck();
        (_slaveGame, _slaveCamp, _slaveMods) = PortTestSetups.Slave();
        (_wangGame, _wangCamp, _wangTdCamp, _wangLooseMap, _wangMods) = PortTestSetups.Wang();
        (_ww2Game, _ww2Camp, _ww2PlatoonCamp, _ww2Mods) = PortTestSetups.WW2GI();
    }

    [Fact]
    public void BloodTest()
    {
        var mods = _bloodMods.StandardMods;

        Raze raze = new();

        raze.BeforeStart(_bloodGame, _bloodCamp);
        var args = raze.GetStartGameArgs(_bloodGame, _bloodCamp, mods, [], true, true);
        var expected = @$" -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -file ""mod_incompatible_with_addon.zip"" -file ""incompatible_mod_with_compatible_version.zip"" -file ""dependent_mod.zip"" -file ""dependent_mod_with_compatible_version.zip"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Saves\Raze\Blood\blood"" -def ""a"" -quick -nosetup";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
                              [GameSearch.Directories]
                              Path=D:/Games/Blood

                              [FileSearch.Directories]
                              Path={Directory.GetCurrentDirectory()}/Data/Addons/Blood/Mods

                              [SoundfontSearch.Directories]
                              """.Replace('\\', '/').Replace("\r\n", "\n"), config.Replace("\r\n", "\n"));
    }

    [Fact]
    public void BloodCPTest()
    {
        var mods = _bloodMods.StandardMods;

        Raze raze = new();

        raze.BeforeStart(_bloodGame, _bloodCamp);
        var args = raze.GetStartGameArgs(_bloodGame, _bloodCpCamp, mods, [], true, true);
        var expected = @$" -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -file ""mod_requires_addon.zip"" -file ""incompatible_mod_with_compatible_version.zip"" -file ""dependent_mod.zip"" -file ""dependent_mod_with_compatible_version.zip"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Saves\Raze\Blood\bloodcp"" -def ""a"" -ini ""CRYPTIC.INI"" -quick -nosetup";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
                              [GameSearch.Directories]
                              Path=D:/Games/Blood

                              [FileSearch.Directories]
                              Path={Directory.GetCurrentDirectory()}/Data/Addons/Blood/Mods

                              [SoundfontSearch.Directories]
                              """.Replace('\\', '/').Replace("\r\n", "\n"), config.Replace("\r\n", "\n"));
    }

    [Fact]
    public void BloodTCTest()
    {
        Raze raze = new();

        raze.BeforeStart(_bloodGame, _bloodTc);
        var args = raze.GetStartGameArgs(_bloodGame, _bloodTc, [], [], true, true);
        var expected = @$" -savedir ""{Directory.GetCurrentDirectory()}\Data\Saves\Raze\Blood\blood-tc"" -def ""a"" -ini ""TC.INI"" -file ""D:\Games\Blood\blood_tc.zip"" -file ""TC.RFF"" -file ""TC.SND"" -quick -nosetup";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
                              [GameSearch.Directories]
                              Path=D:/Games/Blood

                              [FileSearch.Directories]
                              Path={Directory.GetCurrentDirectory()}/Data/Addons/Blood/Mods

                              [SoundfontSearch.Directories]
                              """.Replace('\\', '/').Replace("\r\n", "\n"), config.Replace("\r\n", "\n"));
    }

    [Fact]
    public void BloodTCFolderTest()
    {
        Raze raze = new();

        raze.BeforeStart(_bloodGame, _bloodTcFolder);
        var args = raze.GetStartGameArgs(_bloodGame, _bloodTcFolder, [], [], true, true);
        var expected = @$" -savedir ""{Directory.GetCurrentDirectory()}\Data\Saves\Raze\Blood\blood-tc-folder"" -def ""a"" -ini ""TC.INI"" -file ""D:\Games\Blood\blood_tc_folder"" -file ""TC.RFF"" -file ""TC.SND"" -quick -nosetup";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
                              [GameSearch.Directories]
                              Path=D:/Games/Blood

                              [FileSearch.Directories]
                              Path={Directory.GetCurrentDirectory()}/Data/Addons/Blood/Mods
                              Path=D:/Games/Blood/blood_tc_folder

                              [SoundfontSearch.Directories]
                              """.Replace('\\', '/').Replace("\r\n", "\n"), config.Replace("\r\n", "\n"));
    }

    [Fact]
    public void BloodLooseMapTest()
    {
        var mods = _bloodMods.StandardMods;

        Raze raze = new();

        raze.BeforeStart(_bloodGame, _bloodLooseMap);
        var args = raze.GetStartGameArgs(_bloodGame, _bloodLooseMap, mods, [], true, true);
        var expected = @$" -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -file ""mod_incompatible_with_addon.zip"" -file ""incompatible_mod_with_compatible_version.zip"" -file ""dependent_mod.zip"" -file ""dependent_mod_with_compatible_version.zip"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Saves\Raze\Blood\loose-map"" -def ""a"" -ini ""BLOOD.INI"" -file ""{Directory.GetCurrentDirectory()}\Data\Addons\Blood\Maps"" -map ""LOOSE.MAP"" -quick -nosetup";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
                              [GameSearch.Directories]
                              Path=D:/Games/Blood

                              [FileSearch.Directories]
                              Path={Directory.GetCurrentDirectory()}/Data/Addons/Blood/Mods

                              [SoundfontSearch.Directories]
                              """.Replace('\\', '/').Replace("\r\n", "\n"), config.Replace("\r\n", "\n"));
    }

    [Fact]
    public void DukeTest()
    {
        var mods = _dukeMods.StandardModsWithCons;

        Raze raze = new();

        raze.BeforeStart(_dukeGame, _dukeCamp);
        var args = raze.GetStartGameArgs(_dukeGame, _dukeCamp, mods, [], true, true);

        var expected = $"" +
                       $" -file \"enabled_mod.zip\"" +
                       $" -adddef \"ENABLED1.DEF\"" +
                       $" -adddef \"ENABLED2.DEF\"" +
                       $" -addcon \"ENABLED1.CON\"" +
                       $" -addcon \"ENABLED2.CON\"" +
                       $" -file \"mod_incompatible_with_addon.zip\"" +
                       $" -file \"incompatible_mod_with_compatible_version.zip\"" +
                       $" -file \"dependent_mod.zip\"" +
                       $" -file \"dependent_mod_with_compatible_version.zip\"" +
                       $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Duke3D\\duke3d\"" +
                       $" -def \"a\"" +
                       $" -addon 0" +
                       $" -quick" +
                       $" -nosetup" +
                       $"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
                              [GameSearch.Directories]
                              Path=D:/Games/Duke3D
                              Path=D:/Games/Duke3D/Vaca
                              Path=D:/Games/Duke3D/DC
                              Path=D:/Games/Duke3D/NW

                              [FileSearch.Directories]
                              Path={Directory.GetCurrentDirectory()}/Data/Addons/Duke3D/Mods

                              [SoundfontSearch.Directories]
                              """.Replace('\\', '/').Replace("\r\n", "\n"), config.Replace("\r\n", "\n"));
    }

    [Fact]
    public void DukeWtTest()
    {
        Raze raze = new();

        var args = raze.GetStartGameArgs(_dukeGame, _dukeWtCamp, [], [], true, true);

        var expected = $"" +
                       $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Duke3D\\duke3d_wt\"" +
                       $" -def \"a\"" +
                       $" -addon 0" +
                       $" -quick" +
                       $" -nosetup" +
                       $"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
                              [GameSearch.Directories]
                              Path=D:/Games/DukeWT

                              [FileSearch.Directories]
                              Path={Directory.GetCurrentDirectory()}/Data/Addons/Duke3D/Mods

                              [SoundfontSearch.Directories]
                              """.Replace('\\', '/').Replace("\r\n", "\n"), config.Replace("\r\n", "\n"));
    }

    [Fact]
    public void DukeVacaTest()
    {
        var mods = _dukeMods.AddonModsWithCons;

        Raze raze = new();

        raze.BeforeStart(_dukeGame, _dukeVaca);
        var args = raze.GetStartGameArgs(_dukeGame, _dukeVaca, mods, [], true, true);

        var expected = $"" +
                       $" -file \"enabled_mod.zip\"" +
                       $" -adddef \"ENABLED1.DEF\"" +
                       $" -adddef \"ENABLED2.DEF\"" +
                       $" -addcon \"ENABLED1.CON\"" +
                       $" -addcon \"ENABLED2.CON\"" +
                       $" -file \"mod_requires_addon.zip\"" +
                       $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Duke3D\\dukevaca\"" +
                       $" -def \"a\"" +
                       $" -addon 3" +
                       $" -quick" +
                       $" -nosetup" +
                       $"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
                              [GameSearch.Directories]
                              Path=D:/Games/Duke3D
                              Path=D:/Games/Duke3D/Vaca
                              Path=D:/Games/Duke3D/DC
                              Path=D:/Games/Duke3D/NW

                              [FileSearch.Directories]
                              Path={Directory.GetCurrentDirectory()}/Data/Addons/Duke3D/Mods

                              [SoundfontSearch.Directories]
                              """.Replace('\\', '/').Replace("\r\n", "\n"), config.Replace("\r\n", "\n"));
    }

    [Fact]
    public void DukeTCTest()
    {
        Raze raze = new();

        var args = raze.GetStartGameArgs(_dukeGame, _dukeTcForVaca, [], [], true, true);

        var expected = $"" +
                       $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Duke3D\\duke-tc\"" +
                       $" -def \"TC.DEF\"" +
                       $" -adddef \"TC1.DEF\"" +
                       $" -adddef \"TC2.DEF\"" +
                       $" -addon 3" +
                       $" -con \"TC.CON\"" +
                       $" -addcon \"TC1.CON\"" +
                       $" -addcon \"TC2.CON\"" +
                       $" -file \"{Path.Combine(Directory.GetCurrentDirectory(), "Data", "Duke3D", "Campaigns", "duke_tc.zip")}\"" +
                       $" -quick" +
                       $" -nosetup" +
                       $"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void DukePackedAddonTest()
    {
        Raze raze = new();

        var packedCamp = PortTestSetups.PackedDukeAddonCampaign();
        var zipFilePath = packedCamp.FileInfo!.PathToFile;

        var args = raze.GetStartGameArgs(_dukeGame, packedCamp, [], [], true, true);

        var expected = $"" +
                       $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Duke3D\\packed-camp\"" +
                       $" -def \"a\"" +
                       $" -addon 0" +
                       $" -file \"{zipFilePath}\"" +
                       $" -quick" +
                       $" -nosetup" +
                       $"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }

    [Fact]
    public void DukeLooseMapTest()
    {
        var mods = _dukeMods.StandardModsWithCons;

        var dukeGame = new DukeGame
        {
            Duke64RomPath = null,
            DukeZHRomPath = null,
            DukeWTInstallPath = null,
            GameInstallFolder = Path.Combine("D:", "Games", "Duke3D"),
            AddonsPaths = []
        };

        Raze raze = new();

        raze.BeforeStart(dukeGame, _dukeLooseMap);
        var args = raze.GetStartGameArgs(dukeGame, _dukeLooseMap, mods, [], true, true);

        var expected = $"" +
                       $" -file \"enabled_mod.zip\"" +
                       $" -adddef \"ENABLED1.DEF\"" +
                       $" -adddef \"ENABLED2.DEF\"" +
                       $" -addcon \"ENABLED1.CON\"" +
                       $" -addcon \"ENABLED2.CON\"" +
                       $" -file \"mod_incompatible_with_addon.zip\"" +
                       $" -file \"incompatible_mod_with_compatible_version.zip\"" +
                       $" -file \"dependent_mod.zip\"" +
                       $" -file \"dependent_mod_with_compatible_version.zip\"" +
                       $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Duke3D\\loose-map\"" +
                       $" -def \"a\"" +
                       $" -file \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Duke3D\\Maps\"" +
                       $" -map \"LOOSE.MAP\"" +
                       $" -quick" +
                       $" -nosetup" +
                       $"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
                              [GameSearch.Directories]
                              Path=D:/Games/Duke3D

                              [FileSearch.Directories]
                              Path={Directory.GetCurrentDirectory()}/Data/Addons/Duke3D/Mods

                              [SoundfontSearch.Directories]
                              """.Replace('\\', '/').Replace("\r\n", "\n"), config.Replace("\r\n", "\n"));
    }

    [Fact]
    public void NamTest()
    {
        var mods = _namMods.MinimalMods;

        Raze raze = new();

        raze.BeforeStart(_namGame, _namCamp);
        var args = raze.GetStartGameArgs(_namGame, _namCamp, mods, [], true, true);

        var expected = $"" +
                       $" -file \"enabled_mod.zip\"" +
                       $" -adddef \"ENABLED1.DEF\"" +
                       $" -adddef \"ENABLED2.DEF\"" +
                       $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\NAM\\nam\"" +
                       $" -def \"a\"" +
                       $" -nam" +
                       $" -file NAM.GRP" +
                       $" -con GAME.CON" +
                       $" -quick" +
                       $" -nosetup" +
                       $"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
                              [GameSearch.Directories]
                              Path=D:/Games/NAM

                              [FileSearch.Directories]
                              Path={Directory.GetCurrentDirectory()}/Data/Addons/NAM/Mods

                              [SoundfontSearch.Directories]
                              """.Replace('\\', '/').Replace("\r\n", "\n"), config.Replace("\r\n", "\n"));
    }

    [Fact]
    public void RedneckTest()
    {
        var mods = _redneckMods.MinimalMods;

        Raze raze = new();

        raze.BeforeStart(_redneckGame, _redneckCamp);
        var args = raze.GetStartGameArgs(_redneckGame, _redneckCamp, mods, [], true, true);

        var expected = $"" +
                       $" -file \"enabled_mod.zip\"" +
                       $" -adddef \"ENABLED1.DEF\"" +
                       $" -adddef \"ENABLED2.DEF\"" +
                       $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Redneck\\redneck\"" +
                       $" -def \"a\"" +
                       $" -quick" +
                       $" -nosetup" +
                       $"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
                              [GameSearch.Directories]
                              Path=D:/Games/Redneck

                              [FileSearch.Directories]
                              Path={Directory.GetCurrentDirectory()}/Data/Addons/Redneck/Mods

                              [SoundfontSearch.Directories]
                              """.Replace('\\', '/').Replace("\r\n", "\n"), config.Replace("\r\n", "\n"));
    }

    [Fact]
    public void RedneckAgainTest()
    {
        var mods = _redneckMods.MinimalMods;

        Raze raze = new();

        raze.BeforeStart(_redneckGame, _redneckAgainCamp);
        var args = raze.GetStartGameArgs(_redneckGame, _redneckAgainCamp, mods, [], true, true);

        var expected = $"" +
                       $" -file \"enabled_mod.zip\"" +
                       $" -adddef \"ENABLED1.DEF\"" +
                       $" -adddef \"ENABLED2.DEF\"" +
                       $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Redneck\\ridesagain\"" +
                       $" -def \"a\"" +
                       $" -quick" +
                       $" -nosetup" +
                       $"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
                              [GameSearch.Directories]
                              Path=D:/Games/Again

                              [FileSearch.Directories]
                              Path={Directory.GetCurrentDirectory()}/Data/Addons/Redneck/Mods

                              [SoundfontSearch.Directories]
                              """.Replace('\\', '/').Replace("\r\n", "\n"), config.Replace("\r\n", "\n"));
    }

    [Fact]
    public void SlaveTest()
    {
        var mods = _slaveMods.MinimalMods;

        Raze raze = new();

        raze.BeforeStart(_slaveGame, _slaveCamp);
        var args = raze.GetStartGameArgs(_slaveGame, _slaveCamp, mods, [], true, true);

        var expected = $"" +
                       $" -file \"enabled_mod.zip\"" +
                       $" -adddef \"ENABLED1.DEF\"" +
                       $" -adddef \"ENABLED2.DEF\"" +
                       $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Slave\\slave\"" +
                       $" -def \"a\"" +
                       $" -quick" +
                       $" -nosetup" +
                       $"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
                              [GameSearch.Directories]
                              Path=D:/Games/Slave

                              [FileSearch.Directories]
                              Path={Directory.GetCurrentDirectory()}/Data/Addons/Slave/Mods

                              [SoundfontSearch.Directories]
                              """.Replace('\\', '/').Replace("\r\n", "\n"), config.Replace("\r\n", "\n"));
    }

    [Fact]
    public void WangTest()
    {
        var mods = _wangMods.StandardMods;

        Raze raze = new();

        raze.BeforeStart(_wangGame, _wangCamp);
        var args = raze.GetStartGameArgs(_wangGame, _wangCamp, mods, [], true, true);

        var expected = "" +
                       " -file \"enabled_mod.zip\"" +
                       " -adddef \"ENABLED1.DEF\"" +
                       " -adddef \"ENABLED2.DEF\"" +
                       " -file \"mod_incompatible_with_addon.zip\"" +
                       " -file \"incompatible_mod_with_compatible_version.zip\"" +
                       " -file \"dependent_mod.zip\"" +
                       " -file \"dependent_mod_with_compatible_version.zip\"" +
                       " -file \"feature_mod.zip\"" +
                       $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Wang\\wang\"" +
                       " -def \"a\"" +
                       " -quick" +
                       " -nosetup" +
                       "";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
                              [GameSearch.Directories]
                              Path=D:/Games/Wang

                              [FileSearch.Directories]
                              Path={Directory.GetCurrentDirectory()}/Data/Addons/Wang/Mods

                              [SoundfontSearch.Directories]
                              """.Replace('\\', '/').Replace("\r\n", "\n"), config.Replace("\r\n", "\n"));
    }

    [Fact]
    public void WangTdTest()
    {
        var mods = _wangMods.AddonMods;

        Raze raze = new();

        raze.BeforeStart(_wangGame, _wangTdCamp);
        var args = raze.GetStartGameArgs(_wangGame, _wangTdCamp, mods, [], true, true);

        var expected = "" +
                       " -file \"enabled_mod.zip\"" +
                       " -adddef \"ENABLED1.DEF\"" +
                       " -adddef \"ENABLED2.DEF\"" +
                       " -file \"mod_requires_addon.zip\"" +
                       $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Wang\\twindragon\"" +
                       " -def \"a\"" +
                       " -file \"D:\\Games\\Wang\\TD.zip\"" +
                       " -quick" +
                       " -nosetup" +
                       "";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
                              [GameSearch.Directories]
                              Path=D:/Games/Wang

                              [FileSearch.Directories]
                              Path={Directory.GetCurrentDirectory()}/Data/Addons/Wang/Mods

                              [SoundfontSearch.Directories]
                              """.Replace('\\', '/').Replace("\r\n", "\n"), config.Replace("\r\n", "\n"));
    }

    [Fact]
    public void WangLooseMapTest()
    {
        var mods = _wangMods.StandardMods;

        Raze raze = new();

        raze.BeforeStart(_wangGame, _wangLooseMap);
        var args = raze.GetStartGameArgs(_wangGame, _wangLooseMap, mods, [], true, true);

        var expected = $"" +
                       $" -file \"enabled_mod.zip\"" +
                       $" -adddef \"ENABLED1.DEF\"" +
                       $" -adddef \"ENABLED2.DEF\"" +
                       $" -file \"mod_incompatible_with_addon.zip\"" +
                       $" -file \"incompatible_mod_with_compatible_version.zip\"" +
                       $" -file \"dependent_mod.zip\"" +
                       $" -file \"dependent_mod_with_compatible_version.zip\"" +
                       $" -file \"feature_mod.zip\"" +
                       $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\Wang\\loose-map\"" +
                       $" -def \"a\"" +
                       $" -file \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Wang\\Maps\"" +
                       $" -map \"LOOSE.MAP\"" +
                       $" -quick" +
                       $" -nosetup" +
                       $"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
                              [GameSearch.Directories]
                              Path=D:/Games/Wang

                              [FileSearch.Directories]
                              Path={Directory.GetCurrentDirectory()}/Data/Addons/Wang/Mods

                              [SoundfontSearch.Directories]
                              """.Replace('\\', '/').Replace("\r\n", "\n"), config.Replace("\r\n", "\n"));
    }

    [Fact]
    public void WW2GITest()
    {
        var mods = _ww2Mods.MinimalMods;

        Raze raze = new();

        raze.BeforeStart(_ww2Game, _ww2Camp);
        var args = raze.GetStartGameArgs(_ww2Game, _ww2Camp, mods, [], true, true);

        var expected = "" +
                       " -file \"enabled_mod.zip\"" +
                       " -adddef \"ENABLED1.DEF\"" +
                       " -adddef \"ENABLED2.DEF\"" +
                       $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\WW2GI\\ww2gi\"" +
                       " -def \"a\" -ww2gi" +
                       " -file WW2GI.GRP" +
                       " -con GAME.CON" +
                       " -quick" +
                       " -nosetup" +
                       "";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
                              [GameSearch.Directories]
                              Path=D:/Games/WW2GI

                              [FileSearch.Directories]
                              Path={Directory.GetCurrentDirectory()}/Data/Addons/WW2GI/Mods

                              [SoundfontSearch.Directories]
                              """.Replace('\\', '/').Replace("\r\n", "\n"), config.Replace("\r\n", "\n"));
    }

    [Fact]
    public void WW2GIPlatoonTest()
    {
        var mods = _ww2Mods.MinimalMods;

        Raze raze = new();

        raze.BeforeStart(_ww2Game, _ww2PlatoonCamp);
        var args = raze.GetStartGameArgs(_ww2Game, _ww2PlatoonCamp, mods, [], true, true);

        var expected = "" +
                       " -file \"enabled_mod.zip\"" +
                       " -adddef \"ENABLED1.DEF\"" +
                       " -adddef \"ENABLED2.DEF\"" +
                       $" -savedir \"{Directory.GetCurrentDirectory()}\\Data\\Saves\\Raze\\WW2GI\\platoon\"" +
                       " -def \"a\"" +
                       " -ww2gi" +
                       " -file WW2GI.GRP" +
                       " -file PLATOONL.DAT" +
                       " -con PLATOONL.DEF" +
                       " -quick" +
                       " -nosetup" +
                       "";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
                              [GameSearch.Directories]
                              Path=D:/Games/WW2GI

                              [FileSearch.Directories]
                              Path={Directory.GetCurrentDirectory()}/Data/Addons/WW2GI/Mods

                              [SoundfontSearch.Directories]
                              """.Replace('\\', '/').Replace("\r\n", "\n"), config.Replace("\r\n", "\n"));
    }
}
