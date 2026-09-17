using Core.All.Enums.Addons;
using Games.Games;

namespace Tests.Unit.Sequential;

/// <summary> Tests for <see cref="DukeGame" /> addon detection. </summary>
public sealed class DukeGameAddonTests : IDisposable
{
    /// <summary> Duke Nukem 3D game under test. </summary>
    private readonly DukeGame _game;

    /// <summary> Temporary directory for test files. </summary>
    private readonly string _tempDir;

    /// <summary> Initializes a new instance of the <see cref="DukeGameAddonTests" /> class. </summary>
    public DukeGameAddonTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _game = CreateGame(_tempDir);
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

    /// <summary> Creates a Duke game pointed at the specified install folder. </summary>
    /// <param name="installFolder"> Game install folder. </param>
    /// <returns> The created game. </returns>
    private static DukeGame CreateGame(string? installFolder) => new()
    {
        Duke64RomPath = null,
        DukeZHRomPath = null,
        DukeWTInstallPath = null,
        GameInstallFolder = installFolder
    };

    /// <summary> Creates a mock GRP file at the specified relative path within the temp directory. </summary>
    /// <param name="relativePath"> Relative path for the GRP file. </param>
    /// <returns> The parent directory path. </returns>
    private string CreateGrp(string relativePath)
    {
        var fullPath = Path.Combine(_tempDir, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, "mock grp");

        return Path.GetDirectoryName(fullPath)!;
    }

    /// <summary> Tests that a null install folder yields no addon folders and reports no addons as installed. </summary>
    [Fact]
    public void NullInstallFolder_YieldsNoAddons()
    {
        var game = CreateGame(null);

        Assert.Empty(game.AddonsFolders);
        Assert.Empty(game.AdditionalFolders);
        Assert.False(game.IsDukeDCInstalled);
        Assert.False(game.IsNuclearWinterInstalled);
        Assert.False(game.IsCaribbeanInstalled);
    }

    /// <summary> Tests that Duke DC is found when the GRP file is in the root folder. </summary>
    [Fact]
    public void IsDukeDCInstalled_InRoot_ReturnsTrue()
    {
        _ = CreateGrp("DUKEDC.GRP");

        Assert.True(_game.IsDukeDCInstalled);
        Assert.Equal(_tempDir, _game.AddonsFolders[DukeAddonEnum.DukeDC]);
    }

    /// <summary> Tests that Duke DC is found when the GRP file is in the AddOns folder. </summary>
    [Fact]
    public void IsDukeDCInstalled_InAddOnsFolder_ReturnsTrue()
    {
        _ = CreateGrp(Path.Combine("AddOns", "DUKEDC.GRP"));

        Assert.True(_game.IsDukeDCInstalled);
        Assert.EndsWith("AddOns", _game.AddonsFolders[DukeAddonEnum.DukeDC], StringComparison.OrdinalIgnoreCase);
    }

    /// <summary> Tests that Duke DC is found when the GRP file is in the Megaton addons/dc folder. </summary>
    [Fact]
    public void IsDukeDCInstalled_InMegatonDC_ReturnsTrue()
    {
        _ = CreateGrp(Path.Combine("addons", "dc", "DUKEDC.GRP"));

        Assert.True(_game.IsDukeDCInstalled);
        Assert.EndsWith(Path.Combine("addons", "dc"), _game.AddonsFolders[DukeAddonEnum.DukeDC], StringComparison.OrdinalIgnoreCase);
    }

    /// <summary> Tests that Nuclear Winter is found in the Megaton addons/nw folder. </summary>
    [Fact]
    public void IsNuclearWinterInstalled_InMegatonNW_ReturnsTrue()
    {
        _ = CreateGrp(Path.Combine("addons", "nw", "NWINTER.GRP"));

        Assert.True(_game.IsNuclearWinterInstalled);
        Assert.EndsWith(Path.Combine("addons", "nw"), _game.AddonsFolders[DukeAddonEnum.DukeNW], StringComparison.OrdinalIgnoreCase);
    }

    /// <summary> Tests that Caribbean (Vacation) is found in the Megaton addons/vacation folder. </summary>
    [Fact]
    public void IsCaribbeanInstalled_InMegatonVacation_ReturnsTrue()
    {
        _ = CreateGrp(Path.Combine("addons", "vacation", "VACATION.GRP"));

        Assert.True(_game.IsCaribbeanInstalled);
        Assert.EndsWith(Path.Combine("addons", "vacation"), _game.AddonsFolders[DukeAddonEnum.DukeVaca], StringComparison.OrdinalIgnoreCase);
    }

    /// <summary> Tests that a missing addon is not reported and has no folder entry. </summary>
    [Fact]
    public void IsDukeDCInstalled_NotFound_ReturnsFalse()
    {
        Assert.False(_game.IsDukeDCInstalled);
        Assert.DoesNotContain(DukeAddonEnum.DukeDC, _game.AddonsFolders.Keys);
    }

    /// <summary> Tests that the first matching layout wins when the addon exists in multiple folders. </summary>
    [Fact]
    public void AddonsFolders_MultipleLocations_ReturnsFirstMatch()
    {
        var root = CreateGrp("DUKEDC.GRP");
        _ = CreateGrp(Path.Combine("AddOns", "DUKEDC.GRP"));

        Assert.Equal(root, _game.AddonsFolders[DukeAddonEnum.DukeDC]);
    }

    /// <summary> Tests that repeated access returns the same cached dictionary instance. </summary>
    [Fact]
    public void AddonsFolders_RepeatedAccess_ReturnsCachedInstance()
    {
        _ = CreateGrp("DUKEDC.GRP");

        var first = _game.AddonsFolders;
        var second = _game.AddonsFolders;

        Assert.Same(first, second);
    }

    /// <summary> Tests that additional folders are empty when no addons are detected. </summary>
    [Fact]
    public void AdditionalFolders_NoAddons_IsEmpty()
    {
        Assert.Empty(_game.AdditionalFolders);
    }

    /// <summary> Tests that additional folders contain every detected addon folder. </summary>
    [Fact]
    public void AdditionalFolders_ReturnsAllDetectedFolders()
    {
        _ = CreateGrp(Path.Combine("AddOns", "DUKEDC.GRP"));
        _ = CreateGrp(Path.Combine("addons", "nw", "NWINTER.GRP"));
        _ = CreateGrp(Path.Combine("addons", "vacation", "VACATION.GRP"));

        Assert.Equal(3, _game.AdditionalFolders.Count);
    }

    /// <summary> Tests that two addons sharing a folder produce a single additional folder. </summary>
    [Fact]
    public void AdditionalFolders_AddonsInSameFolder_ReturnsDistinctFolders()
    {
        var folder = CreateGrp(Path.Combine("AddOns", "DUKEDC.GRP"));
        File.WriteAllText(Path.Combine(folder, "NWINTER.GRP"), "mock");

        Assert.Equal(folder, Assert.Single(_game.AdditionalFolders));
    }

    /// <summary> Tests that invalidating the cache forces the filesystem to be scanned again. </summary>
    [Fact]
    public void InvalidateAddonsCache_ThenAccess_RescansFilesystem()
    {
        Assert.Empty(_game.AddonsFolders);

        _ = CreateGrp("DUKEDC.GRP");

        Assert.False(_game.IsDukeDCInstalled);

        _game.InvalidateAddonsCache();

        Assert.True(_game.IsDukeDCInstalled);
        Assert.Equal(_tempDir, _game.AddonsFolders[DukeAddonEnum.DukeDC]);
    }

    /// <summary> Tests that changing the install folder rebuilds the cache. </summary>
    [Fact]
    public void AddonsFolders_InstallFolderChanged_RebuildsCache()
    {
        var otherDir = Path.Combine(_tempDir, "Other");
        _ = Directory.CreateDirectory(otherDir);
        File.WriteAllText(Path.Combine(otherDir, "NWINTER.GRP"), "mock");

        _ = CreateGrp("DUKEDC.GRP");

        Assert.True(_game.IsDukeDCInstalled);
        Assert.False(_game.IsNuclearWinterInstalled);

        _game.GameInstallFolder = otherDir;

        Assert.False(_game.IsDukeDCInstalled);
        Assert.True(_game.IsNuclearWinterInstalled);
        Assert.Equal(otherDir, _game.AddonsFolders[DukeAddonEnum.DukeNW]);
    }
}
