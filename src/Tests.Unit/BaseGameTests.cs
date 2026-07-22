using Core.All.Enums;
using Games.Games;

namespace Tests.Unit;

internal sealed class BaseGameTestProxy : BaseGame
{
    public override GameEnum GameEnum => GameEnum.Duke3D;
    public override string FullName => "Test Game";
    public override string ShortName => "Test";
    protected override IReadOnlyList<string> RequiredFiles => ["TEST.GRP"];
    public override Enum? Skills => null;

    public bool CallIsInstalled(IReadOnlyList<string> files, string? path = null) => IsInstalled(files, path);
    public static IReadOnlyList<string> CallGenerateNumberedFiles(string baseName, string ext, int start, int endExclusive, int padWidth)
        => GenerateNumberedFiles(baseName, ext, start, endExclusive, padWidth);
}

public sealed class BaseGameTests : IDisposable
{
    private readonly string _tempDir;

    public BaseGameTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    [Fact]
    public void CampaignsFolderPath_ContainsGameShortName()
    {
        var game = new BaseGameTestProxy();
        Assert.EndsWith(Path.Combine("Test", "Campaigns"), (string?)game.CampaignsFolderPath);
    }

    [Fact]
    public void MapsFolderPath_ContainsGameShortName()
    {
        var game = new BaseGameTestProxy();
        Assert.EndsWith(Path.Combine("Test", "Maps"), (string?)game.MapsFolderPath);
    }

    [Fact]
    public void ModsFolderPath_ContainsGameShortName()
    {
        var game = new BaseGameTestProxy();
        Assert.EndsWith(Path.Combine("Test", "Mods"), (string?)game.ModsFolderPath);
    }

    [Fact]
    public void AreSkillsAvailable_WhenSkillsIsNull_ReturnsFalse()
    {
        var game = new BaseGameTestProxy();
        Assert.False((bool)game.AreSkillsAvailable);
    }

    [Fact]
    public void IsInstalled_WithNullPathAndNullGameInstallFolder_ReturnsFalse()
    {
        var game = new BaseGameTestProxy();
        Assert.False(game.CallIsInstalled(["DUKE3D.GRP"]));
    }

    [Fact]
    public void IsInstalled_WithExistingFiles_ReturnsTrue()
    {
        var file = Path.Combine(_tempDir, "TEST.GRP");
        File.WriteAllText(file, "");

        var game = new BaseGameTestProxy();
        var result = game.CallIsInstalled(["TEST.GRP"], _tempDir);
        Assert.True(result);
    }

    [Fact]
    public void IsInstalled_WithMissingFiles_ReturnsFalse()
    {
        var game = new BaseGameTestProxy();
        var result = game.CallIsInstalled(["MISSING.GRP"], _tempDir);
        Assert.False(result);
    }

    [Fact]
    public void IsInstalled_PartialMatch_ReturnsFalse()
    {
        var file = Path.Combine(_tempDir, "A.GRP");
        File.WriteAllText(file, "");

        var game = new BaseGameTestProxy();
        var result = game.CallIsInstalled(["A.GRP", "B.GRP"], _tempDir);
        Assert.False(result);
    }

    [Fact]
    public void IsBaseGameInstalled_WhenGameInstallFolderIsNull_ReturnsFalse()
    {
        var game = new BaseGameTestProxy();
        game.GameInstallFolder = null;
        Assert.False(game.IsBaseGameInstalled);
    }

    [Fact]
    public void IsBaseGameInstalled_WhenRequiredFilesExist_ReturnsTrue()
    {
        var file = Path.Combine(_tempDir, "TEST.GRP");
        File.WriteAllText(file, "");

        var game = new BaseGameTestProxy();
        game.GameInstallFolder = _tempDir;
        Assert.True(game.IsBaseGameInstalled);
    }

    [Fact]
    public void GenerateNumberedFiles_Default_MatchesExpectedPattern()
    {
        var files = BaseGameTestProxy.CallGenerateNumberedFiles("FILE", "EXT", 0, 3, 3);
        Assert.Equal(3, files.Count);
        Assert.Equal("FILE000.EXT", files[0]);
        Assert.Equal("FILE001.EXT", files[1]);
        Assert.Equal("FILE002.EXT", files[2]);
    }
}
