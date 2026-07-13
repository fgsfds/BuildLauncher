using Core.All.Enums.Addons;
using Games.Helpers;

namespace Tests.Unit.Sequential;

/// <summary>
///     Tests for the <see cref="DukeAddonDetector" /> class.
/// </summary>
public sealed class DukeAddonDetectorTests : IDisposable
{
    /// <summary>
    ///     Duke addon detector under test.
    /// </summary>
    private readonly DukeAddonDetector _detector;

    /// <summary>
    ///     Temporary directory for test files.
    /// </summary>
    private readonly string _tempDir;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DukeAddonDetectorTests" /> class.
    /// </summary>
    public DukeAddonDetectorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _detector = new DukeAddonDetector();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            try
            {
                Directory.Delete(_tempDir, true);
            }
            catch
            {
                /* best effort */
            }
        }
    }

    /// <summary>
    ///     Creates a mock GRP file at the specified relative path within the temp directory.
    /// </summary>
    /// <param name="relativePath">Relative path for the GRP file.</param>
    /// <returns>The parent directory path.</returns>
    private string CreateGrp(string relativePath)
    {
        var fullPath = Path.Combine(_tempDir, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, "mock grp");

        return Path.GetDirectoryName(fullPath)!;
    }

    /// <summary>
    ///     Tests that a null install folder returns false.
    /// </summary>
    [Fact]
    public void TryFindAddon_NullInstallFolder_ReturnsFalse()
    {
        var result = _detector.TryFindAddon(DukeAddonEnum.DukeDC, null);

        Assert.False(result);
        Assert.Empty(_detector.AddonsPaths);
    }

    /// <summary>
    ///     Tests that a base enum throws an ArgumentOutOfRangeException.
    /// </summary>
    [Fact]
    public void TryFindAddon_BaseEnum_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _detector.TryFindAddon(DukeAddonEnum.Base, _tempDir));
    }

    /// <summary>
    ///     Tests that Duke DC is found when the GRP file is in the root folder.
    /// </summary>
    [Fact]
    public void TryFindAddon_DukeDC_InRoot_FindsAddon()
    {
        CreateGrp("DUKEDC.GRP");

        var result = _detector.TryFindAddon(DukeAddonEnum.DukeDC, _tempDir);

        Assert.True(result);
        Assert.Contains(DukeAddonEnum.DukeDC, _detector.AddonsPaths);
        Assert.Equal(_tempDir, _detector.AddonsPaths[DukeAddonEnum.DukeDC]);
    }

    /// <summary>
    ///     Tests that Duke DC is found when the GRP file is in the AddOns folder.
    /// </summary>
    [Fact]
    public void TryFindAddon_DukeDC_InAddOnsFolder_FindsAddon()
    {
        var path = CreateGrp(Path.Combine("AddOns", "DUKEDC.GRP"));

        var result = _detector.TryFindAddon(DukeAddonEnum.DukeDC, _tempDir);

        Assert.True(result);
        Assert.EndsWith("AddOns", _detector.AddonsPaths[DukeAddonEnum.DukeDC]);
    }

    /// <summary>
    ///     Tests that Duke DC is found when the GRP file is in the Megaton addons/dc folder.
    /// </summary>
    [Fact]
    public void TryFindAddon_DukeDC_InMegatonDC_FindsAddon()
    {
        var path = CreateGrp(Path.Combine("addons", "dc", "DUKEDC.GRP"));

        var result = _detector.TryFindAddon(DukeAddonEnum.DukeDC, _tempDir);

        Assert.True(result);
        Assert.EndsWith(Path.Combine("addons", "dc"), _detector.AddonsPaths[DukeAddonEnum.DukeDC]);
    }

    /// <summary>
    ///     Tests that Nuclear Winter is found in the Megaton addons/nw folder.
    /// </summary>
    [Fact]
    public void TryFindAddon_NuclearWinter_InMegatonNW_FindsAddon()
    {
        CreateGrp(Path.Combine("addons", "nw", "NWINTER.GRP"));

        var result = _detector.TryFindAddon(DukeAddonEnum.DukeNW, _tempDir);

        Assert.True(result);
        Assert.EndsWith(Path.Combine("addons", "nw"), _detector.AddonsPaths[DukeAddonEnum.DukeNW]);
    }

    /// <summary>
    ///     Tests that Caribbean (Vacation) is found in the Megaton addons/vacation folder.
    /// </summary>
    [Fact]
    public void TryFindAddon_Caribbean_InMegatonVacation_FindsAddon()
    {
        CreateGrp(Path.Combine("addons", "vacation", "VACATION.GRP"));

        var result = _detector.TryFindAddon(DukeAddonEnum.DukeVaca, _tempDir);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a missing addon returns false and is removed from the dictionary.
    /// </summary>
    [Fact]
    public void TryFindAddon_NotFound_ReturnsFalseAndRemovesFromDict()
    {
        _detector.AddonsPaths[DukeAddonEnum.DukeDC] = _tempDir;

        var result = _detector.TryFindAddon(DukeAddonEnum.DukeDC, _tempDir);

        Assert.False(result);
        Assert.DoesNotContain(DukeAddonEnum.DukeDC, _detector.AddonsPaths);
    }

    /// <summary>
    ///     Tests that repeated calls preserve the last found path.
    /// </summary>
    [Fact]
    public void TryFindAddon_RepeatedCalls_PreservesLastPath()
    {
        var first = CreateGrp("DUKEDC.GRP");
        var second = Path.Combine(_tempDir, "AddOns");
        Directory.CreateDirectory(second);
        File.WriteAllText(Path.Combine(second, "DUKEDC.GRP"), "mock");

        _detector.TryFindAddon(DukeAddonEnum.DukeDC, _tempDir);

        Assert.Equal(first, _detector.AddonsPaths[DukeAddonEnum.DukeDC]);
    }
}
