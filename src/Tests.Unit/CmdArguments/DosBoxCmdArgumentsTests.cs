using Addons.Addons;
using Core.All.Enums;
using Core.All.Enums.Addons;
using Core.Client.Helpers;
using Games.Games;
using Ports.Ports;
using Tests.Unit.Helpers;

namespace Tests.Unit.CmdArguments;

public sealed class DosBoxCmdArgumentsTests
{
    private readonly BloodCampaign _bloodCamp;
    private readonly BloodCampaign _bloodCpCamp;

    private readonly BloodGame _bloodGame;
    private readonly LooseMap _bloodLooseMap;
    private readonly DukeCampaign _dukeCamp;
    private readonly DukeCampaign _dukeDc;
    private readonly DukeGame _dukeGame;
    private readonly LooseMap _dukeLooseMap;
    private readonly DukeCampaign _dukeNw;
    private readonly DukeCampaign _dukeVaca;
    private readonly DukeCampaign _redneckCamp;

    private readonly RedneckGame _redneckGame;
    private readonly DukeCampaign _ridesAgainCamp;
    private readonly DukeCampaign _route66Camp;
    private readonly GenericCampaign _wangCamp;

    private readonly WangGame _wangGame;

    public DosBoxCmdArgumentsTests()
    {
        (_dukeGame, _dukeCamp, _dukeVaca, _, _, _, _, _dukeDc, _dukeNw, _dukeLooseMap, _) = PortTestSetups.Duke3D();
        (_bloodGame, _bloodCamp, _, _bloodCpCamp, _, _, _, _, _, _bloodLooseMap, _) = PortTestSetups.Blood();
        (_redneckGame, _redneckCamp, _ridesAgainCamp, _route66Camp, _) = PortTestSetups.Redneck();
        (_wangGame, _wangCamp, _, _, _) = PortTestSetups.Wang();
    }

    [Fact]
    public void DukeBaseGameTest()
    {
        DosBox dosBox = new();
        var args = dosBox.GetStartGameArgs(_dukeGame, _dukeCamp, [], [], true, true);

        var expected = $"" +
                       $" --noconsole -c \"cycles max\" -c \"core dynamic\"" +
                       $" -c \"mount c \\\"{_dukeGame.GameInstallFolder}\"\" -c \"c:\"" +
                       $" -c DUKE3D.EXE" +
                       $" -c \"exit\"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void DukeVacaTest()
    {
        DosBox dosBox = new();
        var args = dosBox.GetStartGameArgs(_dukeGame, _dukeVaca, [], [], true, true);
        var vacaPath = _dukeGame.AddonsPaths[DukeAddonEnum.DukeVaca];

        var expected = $"" +
                       $" --noconsole -c \"cycles max\" -c \"core dynamic\"" +
                       $" -c \"mount c \\\"{_dukeGame.GameInstallFolder}\"\" -c \"c:\"" +
                       $" -c \"mount d \\\"{vacaPath}\"\"" +
                       $" -c \"VACATION.EXE /gd:\\\\VACATION.GRP /xd:\\\\VACATION.CON\"" +
                       $" -c \"exit\"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void DukeDcTest()
    {
        DosBox dosBox = new();
        var args = dosBox.GetStartGameArgs(_dukeGame, _dukeDc, [], [], true, true);
        var dcPath = _dukeGame.AddonsPaths[DukeAddonEnum.DukeDC];

        var expected = $"" +
                       $" --noconsole -c \"cycles max\" -c \"core dynamic\"" +
                       $" -c \"mount c \\\"{_dukeGame.GameInstallFolder}\"\" -c \"c:\"" +
                       $" -c \"mount d \\\"{dcPath}\"\"" +
                       $" -c \"DUKE3D.EXE /gd:\\\\DUKEDC.GRP /xd:\\\\DUKEDC.CON\"" +
                       $" -c \"exit\"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void DukeNwTest()
    {
        DosBox dosBox = new();
        var args = dosBox.GetStartGameArgs(_dukeGame, _dukeNw, [], [], true, true);
        var nwPath = _dukeGame.AddonsPaths[DukeAddonEnum.DukeNW];

        var expected = $"" +
                       $" --noconsole -c \"cycles max\" -c \"core dynamic\"" +
                       $" -c \"mount c \\\"{_dukeGame.GameInstallFolder}\"\" -c \"c:\"" +
                       $" -c \"mount d \\\"{nwPath}\"\"" +
                       $" -c \"DUKE3D.EXE /gd:\\\\NWINTER.GRP /xd:\\\\NWINTER.CON\"" +
                       $" -c \"exit\"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void DukeLooseMapTest()
    {
        DosBox dosBox = new();
        var args = dosBox.GetStartGameArgs(_dukeGame, _dukeLooseMap, [], [], true, true);

        var expected = $"" +
                       $" --noconsole -c \"cycles max\" -c \"core dynamic\"" +
                       $" -c \"mount c \\\"{_dukeGame.GameInstallFolder}\"\" -c \"c:\"" +
                       $" -c \"mount d \\\"{_dukeGame.MapsFolderPath}\"\"" +
                       $" -c \"DUKE3D.EXE -map d:\\\\LOOSE.MAP\"" +
                       $" -c \"exit\"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void BloodBaseGameTest()
    {
        DosBox dosBox = new();
        var args = dosBox.GetStartGameArgs(_bloodGame, _bloodCamp, [], [], true, true);

        var expected = $"" +
                       $" --noconsole -c \"cycles max\" -c \"core dynamic\"" +
                       $" -c \"mount c \\\"{_bloodGame.GameInstallFolder}\"\" -c \"c:\"" +
                       $" -c BLOOD.EXE" +
                       $" -c \"exit\"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void BloodCPTest()
    {
        DosBox dosBox = new();
        var args = dosBox.GetStartGameArgs(_bloodGame, _bloodCpCamp, [], [], true, true);

        var expected = $"" +
                       $" --noconsole -c \"cycles max\" -c \"core dynamic\"" +
                       $" -c \"mount c \\\"{_bloodGame.GameInstallFolder}\"\" -c \"c:\"" +
                       $" -c CRYPTIC.EXE" +
                       $" -c \"exit\"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void BloodTCFolderTest()
    {
        var addonDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var gameDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        Directory.CreateDirectory(addonDir);
        Directory.CreateDirectory(gameDir);

        var addonFile1 = Path.Combine(addonDir, "BLOOD.EXE");
        var addonFile2 = Path.Combine(addonDir, "BLOOD.RFF");

        var originalFile1 = Path.Combine(gameDir, "BLOOD.EXE");
        var originalFile2 = Path.Combine(addonDir, "BLOOD.RFF");

        File.WriteAllText(addonFile1, "");
        File.WriteAllText(addonFile2, "");
        File.WriteAllText(originalFile1, "");

        try
        {
            var bloodGame = new BloodGame
            {
                GameInstallFolder = gameDir
            };

            var bloodTcFolder = new BloodCampaign
            {
                AddonId = new("blood-tc-folder", "1.0"),
                Type = AddonTypeEnum.TC,
                Title = "Blood TC Folder",
                SupportedGame = new(GameEnum.Blood, null, null),
                FileInfo = new(addonDir, "addon.json"),
                GridImageHash = null,
                PreviewImageHash = null,
                Description = null,
                Author = null,
                ReleaseDate = null,
                MainDef = null,
                AdditionalDefs = null,
                INI = "BLOODTC.INI",
                RFF = "BLOODTC.RFF",
                SND = "BLOODTC.SND",
                StartMap = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                RequiredFeatures = null,
                Executables = null,
                Options = null
            };

            DosBox dosBox = new();
            var args = dosBox.GetStartGameArgs(bloodGame, bloodTcFolder, [], [], true, true);

            var expected = $"" +
                           $" --noconsole -c \"cycles max\" -c \"core dynamic\"" +
                           $" -c \"mount c \\\"{ClientProperties.TempFolderPath}\"\" -c \"c:\"" +
                           $" -c \"BLOOD.EXE -ini BLOODTC.INI -RFF BLOODTC.RFF -snd BLOODTC.SND\"" +
                           $" -c \"exit\"";

            NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
            Assert.Equal(expected, args);

            Assert.True(File.Exists(addonFile1));
            Assert.True(File.Exists(addonFile2));
            Assert.True(File.Exists(originalFile1));
            Assert.True(File.Exists(originalFile2));
        }
        finally
        {
            if (Directory.Exists(addonDir))
            {
                Directory.Delete(addonDir, true);
            }

            if (Directory.Exists(gameDir))
            {
                Directory.Delete(gameDir, true);
            }
        }
    }

    [Fact]
    public void BloodLooseMapTest()
    {
        DosBox dosBox = new();
        var args = dosBox.GetStartGameArgs(_bloodGame, _bloodLooseMap, [], [], true, true);

        var expected = $"" +
                       $" --noconsole -c \"cycles max\" -c \"core dynamic\"" +
                       $" -c \"mount c \\\"{_bloodGame.GameInstallFolder}\"\" -c \"c:\"" +
                       $" -c \"mount d \\\"{_bloodGame.MapsFolderPath}\"\"" +
                       $" -c \"BLOOD.EXE -map d:\\\\LOOSE.MAP\"" +
                       $" -c \"exit\"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void RedneckBaseTest()
    {
        DosBox dosBox = new();
        var args = dosBox.GetStartGameArgs(_redneckGame, _redneckCamp, [], [], true, true);

        var expected = $"" +
                       $" --noconsole -c \"cycles max\" -c \"core dynamic\"" +
                       $" -c \"mount c \\\"{_redneckGame.GameInstallFolder}\"\" -c \"c:\"" +
                       $" -c RR.EXE" +
                       $" -c \"exit\"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void RidesAgainTest()
    {
        DosBox dosBox = new();
        var args = dosBox.GetStartGameArgs(_redneckGame, _ridesAgainCamp, [], [], true, true);

        var expected = $"" +
                       $" --noconsole -c \"cycles max\" -c \"core dynamic\"" +
                       $" -c \"mount c \\\"{_redneckGame.AgainInstallPath}\"\" -c \"c:\"" +
                       $" -c RA.EXE" +
                       $" -c \"exit\"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void Route66Test()
    {
        DosBox dosBox = new();
        var args = dosBox.GetStartGameArgs(_redneckGame, _route66Camp, [], [], true, true);

        var expected = $"" +
                       $" --noconsole -c \"cycles max\" -c \"core dynamic\"" +
                       $" -c \"mount c \\\"{_redneckGame.GameInstallFolder}\"\" -c \"c:\"" +
                       $" -c ROUTE66.EXE" +
                       $" -c \"exit\"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void WangBaseTest()
    {
        DosBox dosBox = new();
        var args = dosBox.GetStartGameArgs(_wangGame, _wangCamp, [], [], true, true);

        var expected = $"" +
                       $" --noconsole -c \"cycles max\" -c \"core dynamic\"" +
                       $" -c \"mount c \\\"{_wangGame.GameInstallFolder}\"\" -c \"c:\"" +
                       $" -c Sw.EXE" +
                       $" -c \"exit\"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }

    [Fact]
    public void BloodTC_NullFileInfo_FallsThroughToBaseGame()
    {
        var bloodTcNull = new BloodCampaign
        {
            AddonId = new("blood-tc", "1.0"),
            Type = AddonTypeEnum.TC,
            Title = "Blood TC",
            SupportedGame = new(GameEnum.Blood, null, null),
            FileInfo = null,
            GridImageHash = null,
            PreviewImageHash = null,
            Description = null,
            Author = null,
            ReleaseDate = null,
            MainDef = null,
            AdditionalDefs = null,
            INI = "TC.INI",
            RFF = "TC.RFF",
            SND = "TC.SND",
            StartMap = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            RequiredFeatures = null,
            Executables = null,
            Options = null
        };

        DosBox dosBox = new();
        var args = dosBox.GetStartGameArgs(_bloodGame, bloodTcNull, [], [], true, true);

        // Should fall through to base Blood game args instead of NRE
        var expected = $"" +
                       $" --noconsole -c \"cycles max\" -c \"core dynamic\"" +
                       $" -c \"mount c \\\"{_bloodGame.GameInstallFolder}\"\" -c \"c:\"" +
                       $" -c BLOOD.EXE" +
                       $" -c \"exit\"";

        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);
        Assert.Equal(expected, args);
    }
}
