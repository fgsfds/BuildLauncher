using Core.Client.Helpers;
using Tests.Unit.Helpers;

namespace Tests.Unit;

public sealed class AddonFilePathWrapperTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var wrapper = new AddonFilePathWrapper(@"C:\addons\myaddon", "manifest.json");

        Assert.Equal(@"C:\addons\myaddon", NormalizerHelper.NormalizePath(wrapper.PathToFolder));
        Assert.Equal("manifest.json", NormalizerHelper.NormalizePath(wrapper.FileName));
        Assert.Equal(@"C:\addons\myaddon\manifest.json", NormalizerHelper.NormalizePath(wrapper.PathToFile));
    }

    [Fact]
    public void Constructor_ZipPath_SetsFolderToParent()
    {
        var wrapper = new AddonFilePathWrapper(@"C:\addons\pack.zip", "addon.json");

        Assert.Equal(@"C:\addons", NormalizerHelper.NormalizePath(wrapper.PathToFolder));
        Assert.Equal("pack.zip", NormalizerHelper.NormalizePath(wrapper.FileName));
        Assert.Equal(@"C:\addons\pack.zip", NormalizerHelper.NormalizePath(wrapper.PathToFile));
    }

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

    [Fact]
    public void IsJson_ReturnsFalse_WhenPathIsNotFolder()
    {
        var wrapper = new AddonFilePathWrapper(@"C:\addons\myaddon.zip", "info.json");

        Assert.False(wrapper.IsJson);
    }

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

    [Fact]
    public void IsMap_ReturnsFalse_WhenPathIsNotFolder()
    {
        var wrapper = new AddonFilePathWrapper(@"C:\addons\myaddon.zip", "level.map");

        Assert.False(wrapper.IsMap);
    }

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

    [Fact]
    public void IsGrpInfo_ReturnsFalse_WhenPathIsNotFolder()
    {
        var wrapper = new AddonFilePathWrapper(@"C:\addons\myaddon.zip", "info.grpinfo");

        Assert.False(wrapper.IsGrpInfo);
    }

    [Theory]
    [InlineData(@"C:\addons\myaddon", "manifest.json", @"C:\addons\myaddon\manifest.json")]
    [InlineData(@"C:\addons\folder", "file.map", @"C:\addons\folder\file.map")]
    public void PathToFile_ForFolder_ReturnsCombinedPath(string path, string fileName, string expected)
    {
        var wrapper = new AddonFilePathWrapper(path, fileName);

        Assert.Equal(expected, NormalizerHelper.NormalizePath(wrapper.PathToFile));
    }

    [Fact]
    public void PathToFile_ForZip_ReturnsZipPath()
    {
        var wrapper = new AddonFilePathWrapper(@"C:\addons\pack.zip", "addon.json");

        Assert.Equal(@"C:\addons\pack.zip", NormalizerHelper.NormalizePath(wrapper.PathToFile));
    }

    [Theory]
    [InlineData(@"C:\addons\myaddon")]
    [InlineData(@"C:\addons\folder")]
    public void PathToFolder_ForFolder_ReturnsSamePath(string path)
    {
        var wrapper = new AddonFilePathWrapper(path, "manifest.json");

        Assert.Equal(path, NormalizerHelper.NormalizePath(wrapper.PathToFolder));
    }

    [Fact]
    public void PathToFolder_ForZip_ReturnsParentDirectory()
    {
        var wrapper = new AddonFilePathWrapper(@"C:\addons\pack.zip", "addon.json");

        Assert.Equal(@"C:\addons", NormalizerHelper.NormalizePath(wrapper.PathToFolder));
    }

    [Fact]
    public void FileName_ForFolder_ReturnsManifestName()
    {
        var wrapper = new AddonFilePathWrapper(@"C:\addons\myaddon", "manifest.json");

        Assert.Equal("manifest.json", NormalizerHelper.NormalizePath(wrapper.FileName));
    }

    [Fact]
    public void FileName_ForZip_ReturnsZipName()
    {
        var wrapper = new AddonFilePathWrapper(@"C:\addons\pack.zip", "addon.json");

        Assert.Equal("pack.zip", NormalizerHelper.NormalizePath(wrapper.FileName));
    }

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

    [Fact]
    public void WithChangedFolder_OriginalWrapperIsUnchanged()
    {
        var original = new AddonFilePathWrapper(@"C:\addons\original", "manifest.json");
        _ = original.WithChangedFolder(@"D:\new");

        Assert.Equal(@"C:\addons\original", NormalizerHelper.NormalizePath(original.PathToFolder));
        Assert.Equal("manifest.json", NormalizerHelper.NormalizePath(original.FileName));
    }

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
}
