using Core.Client.Helpers;
using Tests.Unit.Helpers;

namespace Tests.Unit;

/// <summary>
///     Tests for the <see cref="AddonFilePathWrapper" /> record.
/// </summary>
public sealed class AddonFilePathWrapperTests
{
    /// <summary>
    ///     Tests that <see cref="AddonFilePathWrapper.IsFolder" /> returns the expected value for various paths.
    /// </summary>
    [Theory]
    [InlineData(@"C:\addons\myaddon", true)]
    [InlineData(@"C:\addons\myaddon.zip", false)]
    [InlineData(@"C:\addons\myaddon.map", false)]
    [InlineData(@"C:\addons\myaddon.json", false)]
    public void IsFolder_ReturnsExpected(string path, bool expected)
    {
        var wrapper = new AddonFilePathWrapper(path, "manifest.json");

        Assert.Equal(expected, wrapper.IsFolder);
    }

    /// <summary>
    ///     Tests that <see cref="AddonFilePathWrapper.IsJson" /> returns the expected value for various file names.
    /// </summary>
    [Theory]
    [InlineData(@"C:\addons\myaddon", "info.json", true)]
    [InlineData(@"C:\addons\myaddon", "info.JSON", true)]
    [InlineData(@"C:\addons\myaddon", "info.JsOn", true)]
    [InlineData(@"C:\addons\myaddon", "info.xml", false)]
    [InlineData(@"C:\addons\myaddon", "json", false)]
    public void IsJson_ReturnsExpected(string path, string fileName, bool expected)
    {
        var wrapper = new AddonFilePathWrapper(path, fileName);

        Assert.Equal(expected, wrapper.IsJson);
    }

    /// <summary>
    ///     Tests that <see cref="AddonFilePathWrapper.IsZip" /> returns the expected value for various paths.
    /// </summary>
    [Theory]
    [InlineData(@"C:\addons\myaddon.zip", true)]
    [InlineData(@"C:\addons\myaddon.ZIP", true)]
    [InlineData(@"C:\addons\myaddon.Zip", true)]
    [InlineData(@"C:\addons\myaddon", false)]
    [InlineData(@"C:\addons\myaddon.json", false)]
    [InlineData(@"C:\addons\myaddon.map", false)]
    public void IsZip_ReturnsExpected(string path, bool expected)
    {
        var wrapper = new AddonFilePathWrapper(path, "manifest.json");

        Assert.Equal(expected, wrapper.IsZip);
    }

    /// <summary>
    ///     Tests that <see cref="AddonFilePathWrapper.IsMap" /> returns the expected value for various file names.
    /// </summary>
    [Theory]
    [InlineData(@"C:\addons\myaddon", "level.map", true)]
    [InlineData(@"C:\addons\myaddon", "level.MAP", true)]
    [InlineData(@"C:\addons\myaddon", "level.Map", true)]
    [InlineData(@"C:\addons\myaddon", "level.txt", false)]
    [InlineData(@"C:\addons\myaddon", "map", false)]
    public void IsMap_ReturnsExpected(string path, string fileName, bool expected)
    {
        var wrapper = new AddonFilePathWrapper(path, fileName);

        Assert.Equal(expected, wrapper.IsMap);
    }

    /// <summary>
    ///     Tests that <see cref="AddonFilePathWrapper.IsGrpInfo" /> returns the expected value for various file names.
    /// </summary>
    [Theory]
    [InlineData(@"C:\addons\myaddon", "info.grpinfo", true)]
    [InlineData(@"C:\addons\myaddon", "info.GRPINFO", true)]
    [InlineData(@"C:\addons\myaddon", "info.GrpInfo", true)]
    [InlineData(@"C:\addons\myaddon", "info.txt", false)]
    [InlineData(@"C:\addons\myaddon", "grpinfo", false)]
    public void IsGrpInfo_ReturnsExpected(string path, string fileName, bool expected)
    {
        var wrapper = new AddonFilePathWrapper(path, fileName);

        Assert.Equal(expected, wrapper.IsGrpInfo);
    }

    /// <summary>
    ///     Tests that <see cref="AddonFilePathWrapper.WithChangedFolder" /> returns a new wrapper with an updated folder path.
    /// </summary>
    [Fact]
    public void WithChangedFolder_FolderPath_ReturnsNewWrapperWithUpdatedPath()
    {
        var wrapper = new AddonFilePathWrapper(@"C:\addons\old", "manifest.json");
        var updated = wrapper.WithChangedFolder(@"D:\new\path");

        Assert.Equal(@"D:\new\path", NormalizerHelper.NormalizePath(updated.PathToFolder));
        Assert.Equal(@"D:\new\path\manifest.json", NormalizerHelper.NormalizePath(updated.PathToFile));
        Assert.Equal("manifest.json", NormalizerHelper.NormalizePath(updated.FileName));
        Assert.True(updated.IsFolder);
    }

    /// <summary>
    ///     Tests that <see cref="AddonFilePathWrapper.WithChangedFolder" /> returns a new wrapper when the original is a zip.
    /// </summary>
    [Fact]
    public void WithChangedFolder_ZipPath_ReturnsNewWrapperWithUpdatedPath()
    {
        var wrapper = new AddonFilePathWrapper(@"C:\addons\pack.zip", "addon.json");
        var updated = wrapper.WithChangedFolder(@"D:\mods\newpack.zip");

        Assert.Equal(@"D:\mods", NormalizerHelper.NormalizePath(updated.PathToFolder));
        Assert.Equal(@"D:\mods\newpack.zip", NormalizerHelper.NormalizePath(updated.PathToFile));
        Assert.Equal("newpack.zip", NormalizerHelper.NormalizePath(updated.FileName));
        Assert.True(updated.IsZip);
    }

    /// <summary>
    ///     Tests that <see cref="AddonFilePathWrapper.WithChangedFolder" /> correctly transitions from folder to zip.
    /// </summary>
    [Fact]
    public void WithChangedFolder_FromFolderToZip_UpdatesTypeFlags()
    {
        var folder = new AddonFilePathWrapper(@"C:\addons\myaddon", "manifest.json");
        var zipped = folder.WithChangedFolder(@"C:\addons\myaddon.zip");

        Assert.True(zipped.IsZip);
        Assert.False(zipped.IsFolder);
        Assert.Equal("myaddon.zip", NormalizerHelper.NormalizePath(zipped.FileName));
        Assert.Equal(@"C:\addons", NormalizerHelper.NormalizePath(zipped.PathToFolder));
    }

    /// <summary>
    ///     Tests that <see cref="AddonFilePathWrapper.WithChangedFolder" /> does not mutate the original wrapper.
    /// </summary>
    [Fact]
    public void WithChangedFolder_OriginalWrapperIsUnchanged()
    {
        var original = new AddonFilePathWrapper(@"C:\addons\original", "manifest.json");
        _ = original.WithChangedFolder(@"D:\new");

        Assert.Equal(@"C:\addons\original", NormalizerHelper.NormalizePath(original.PathToFolder));
        Assert.Equal("manifest.json", NormalizerHelper.NormalizePath(original.FileName));
    }

    /// <summary>
    ///     Tests that the record implements value-based equality correctly.
    /// </summary>
    [Fact]
    public void RecordEquality_ByValue()
    {
        var a = new AddonFilePathWrapper(@"C:\addons\a", "m.json");
        var b = new AddonFilePathWrapper(@"C:\addons\a", "m.json");
        var c = new AddonFilePathWrapper(@"C:\addons\a", "n.json");

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, c);
    }

    /// <summary>
    ///     Tests that the constructor normalizes forward slashes to native separators.
    /// </summary>
    [Fact]
    public void Constructor_NormalizesForwardSlashes()
    {
        var wrapper = new AddonFilePathWrapper("C:/addons/myaddon", "addon.json");

        Assert.Equal("C:" + Path.DirectorySeparatorChar + "addons" + Path.DirectorySeparatorChar + "myaddon", wrapper.PathToFolder);
    }

    /// <summary>
    ///     Tests that the constructor normalizes mixed forward and backslashes.
    /// </summary>
    [Fact]
    public void Constructor_NormalizesMixedSlashes()
    {
        var wrapper = new AddonFilePathWrapper("C:/addons\\subfolder", "manifest.json");

        var expected = "C:" + Path.DirectorySeparatorChar + "addons" + Path.DirectorySeparatorChar + "subfolder";
        Assert.Equal(expected, wrapper.PathToFolder);
    }

    /// <summary>
    ///     Tests that wrappers with the same logical path but different separator styles are equal.
    /// </summary>
    [Fact]
    public void DifferentSeparatorForms_SameLogicalPath_AreEqual()
    {
        var a = new AddonFilePathWrapper(@"C:\addons\a", "m.json");
        var b = new AddonFilePathWrapper("C:/addons/a", "m.json");

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    /// <summary>
    ///     Tests that wrappers with different separator styles resolve to the same dictionary key.
    /// </summary>
    [Fact]
    public void DifferentSeparatorForms_SameLogicalPath_SameDictionaryKey()
    {
        var dict = new Dictionary<AddonFilePathWrapper, string>();

        var key1 = new AddonFilePathWrapper(@"C:\addons\a", "m.json");
        var key2 = new AddonFilePathWrapper("C:/addons/a", "m.json");

        dict[key1] = "value";

        Assert.True(dict.ContainsKey(key2));
        Assert.Equal("value", dict[key2]);
    }
}
