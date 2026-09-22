using Core.All.Enums;
using Core.All.Enums.Addons;
using Games.Games;

namespace Tests.Unit;

internal sealed class BaseGameTestProxy : BaseGame
{
    public override GameEnum GameEnum => GameEnum.Duke3D;
    public override string FullName => "Test Game";
    public override string ShortName => "Test";
    protected override IReadOnlyCollection<string> RequiredFiles { get; } = ["TEST.GRP"];
    public override Enum? Skills => null;

    /// <summary> Detected addon folders returned by <see cref="DetectAddonsFolders" /> when set. </summary>
    public IReadOnlyDictionary<Enum, string>? AddonsFoldersToReturn { get; set; }

    /// <summary> Returns <see cref="AddonsFoldersToReturn" /> when set, otherwise the base implementation. </summary>
    /// <returns> The detected addon folders. </returns>
    protected override IReadOnlyDictionary<Enum, string> DetectAddonsFolders() => AddonsFoldersToReturn ?? base.DetectAddonsFolders();

    /// <summary> Invokes the protected <see cref="BaseGame.IsInstalled" /> method. </summary>
    /// <param name="files"> The required files to look for. </param>
    /// <param name="path"> The folder to search, or the game install folder when null. </param>
    /// <returns> True when all files exist. </returns>
    public bool CallIsInstalled(IReadOnlyCollection<string> files, string? path = null) => IsInstalled(files, path);
    public static IReadOnlyCollection<string> CallGenerateNumberedFiles(string baseName, string ext, int start, int endExclusive, int padWidth)
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
        Assert.EndsWith(Path.Combine("Test", "Campaigns"), (string?)game.CampaignsFolderPath, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MapsFolderPath_ContainsGameShortName()
    {
        var game = new BaseGameTestProxy();
        Assert.EndsWith(Path.Combine("Test", "Maps"), (string?)game.MapsFolderPath, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ModsFolderPath_ContainsGameShortName()
    {
        var game = new BaseGameTestProxy();
        Assert.EndsWith(Path.Combine("Test", "Mods"), (string?)game.ModsFolderPath, StringComparison.OrdinalIgnoreCase);
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
        Assert.Equal("FILE000.EXT", files.ElementAt(0));
        Assert.Equal("FILE001.EXT", files.ElementAt(1));
        Assert.Equal("FILE002.EXT", files.ElementAt(2));
    }

    /// <summary> Addons mapping to the same folder with different casing are collapsed to a single entry. </summary>
    [Fact]
    public void AdditionalFolders_AddonsInSameFolderDifferentCasing_Deduplicates()
    {
        var game = new BaseGameTestProxy
        {
            AddonsFoldersToReturn = new Dictionary<Enum, string>
            {
                [DukeAddonEnum.DukeDC] = @"C:\Games\Duke\AddOns",
                [DukeAddonEnum.DukeNW] = @"c:\games\duke\addons"
            }
        };

        Assert.Single(game.AdditionalFolders);
    }
}
