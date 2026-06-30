using Core.All.Enums.Addons;
using Games.Helpers;

namespace Tests.Unit.Sync;

[Collection("Sync")]
public sealed class DukeAddonDetectorTests : IDisposable
{
    private readonly DukeAddonDetector _detector;
    private readonly string _tempDir;

    public DukeAddonDetectorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _detector = new DukeAddonDetector();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            try { Directory.Delete(_tempDir, true); }
            catch
            {
                /* best effort */
            }
        }
    }

    private string CreateGrp(string relativePath)
    {
        var fullPath = Path.Combine(_tempDir, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, "mock grp");

        return Path.GetDirectoryName(fullPath)!;
    }

    [Fact]
    public void TryFindAddon_NullInstallFolder_ReturnsFalse()
    {
        var result = _detector.TryFindAddon(DukeAddonEnum.DukeDC, null);

        Assert.False(result);
        Assert.Empty(_detector.AddonsPaths);
    }

    [Fact]
    public void TryFindAddon_BaseEnum_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _detector.TryFindAddon(DukeAddonEnum.Base, _tempDir));
    }

    [Fact]
    public void TryFindAddon_DukeDC_InRoot_FindsAddon()
    {
        CreateGrp("DUKEDC.GRP");

        var result = _detector.TryFindAddon(DukeAddonEnum.DukeDC, _tempDir);

        Assert.True(result);
        Assert.Contains(DukeAddonEnum.DukeDC, _detector.AddonsPaths);
        Assert.Equal(_tempDir, _detector.AddonsPaths[DukeAddonEnum.DukeDC]);
    }

    [Fact]
    public void TryFindAddon_DukeDC_InAddOnsFolder_FindsAddon()
    {
        var path = CreateGrp(Path.Combine("AddOns", "DUKEDC.GRP"));

        var result = _detector.TryFindAddon(DukeAddonEnum.DukeDC, _tempDir);

        Assert.True(result);
        Assert.EndsWith("AddOns", _detector.AddonsPaths[DukeAddonEnum.DukeDC]);
    }

    [Fact]
    public void TryFindAddon_DukeDC_InMegatonDC_FindsAddon()
    {
        var path = CreateGrp(Path.Combine("addons", "dc", "DUKEDC.GRP"));

        var result = _detector.TryFindAddon(DukeAddonEnum.DukeDC, _tempDir);

        Assert.True(result);
        Assert.EndsWith(Path.Combine("addons", "dc"), _detector.AddonsPaths[DukeAddonEnum.DukeDC]);
    }

    [Fact]
    public void TryFindAddon_NuclearWinter_InMegatonNW_FindsAddon()
    {
        CreateGrp(Path.Combine("addons", "nw", "NWINTER.GRP"));

        var result = _detector.TryFindAddon(DukeAddonEnum.DukeNW, _tempDir);

        Assert.True(result);
        Assert.EndsWith(Path.Combine("addons", "nw"), _detector.AddonsPaths[DukeAddonEnum.DukeNW]);
    }

    [Fact]
    public void TryFindAddon_Caribbean_InMegatonVacation_FindsAddon()
    {
        CreateGrp(Path.Combine("addons", "vacation", "VACATION.GRP"));

        var result = _detector.TryFindAddon(DukeAddonEnum.DukeVaca, _tempDir);

        Assert.True(result);
    }

    [Fact]
    public void TryFindAddon_NotFound_ReturnsFalseAndRemovesFromDict()
    {
        _detector.AddonsPaths[DukeAddonEnum.DukeDC] = _tempDir;

        var result = _detector.TryFindAddon(DukeAddonEnum.DukeDC, _tempDir);

        Assert.False(result);
        Assert.DoesNotContain(DukeAddonEnum.DukeDC, _detector.AddonsPaths);
    }

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
