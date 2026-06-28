using Core.All.Enums;
using Core.All.Serializable.Addon;
using Core.Client.Helpers;

namespace Tests.Unit;

public sealed class ParsedAddonFileTests
{
    private static ParsedAddonFile Create(string path, string fileName, string? manifestId = null)
    {
        return new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(path, fileName),
            SupportedGame = GameEnum.Duke3D,
            Manifest = manifestId is not null ? new AddonManifestJsonModel
            {
                Id = manifestId,
                Title = manifestId,
                Version = "1.0",
                AddonType = AddonTypeEnum.TC,
                SupportedGame = new SupportedGameJsonModel { Game = GameEnum.Duke3D },
            } : null,
            GridHash = 12345,
            PreviewHash = 67890,
        };
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var a = Create(@"C:\addons\a", "addon.json", "test");
        var b = Create(@"C:\addons\a", "addon.json", "test");

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void RecordEquality_DifferentFileInfo_AreNotEqual()
    {
        var a = Create(@"C:\addons\a", "addon.json", "test");
        var b = Create(@"C:\addons\b", "addon.json", "test");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void RecordEquality_DifferentManifest_AreNotEqual()
    {
        var a = Create(@"C:\addons\a", "addon.json", "test-a");
        var b = Create(@"C:\addons\a", "addon.json", "test-b");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void RecordEquality_DifferentGridHash_AreNotEqual()
    {
        var a = Create(@"C:\addons\a", "addon.json", "test");
        var b = a with { GridHash = 99999 };

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void RecordEquality_DifferentPreviewHash_AreNotEqual()
    {
        var a = Create(@"C:\addons\a", "addon.json", "test");
        var b = a with { PreviewHash = 11111 };

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void RecordEquality_DifferentSupportedGame_AreNotEqual()
    {
        var a = Create(@"C:\addons\a", "addon.json", "test");
        var b = a with { SupportedGame = GameEnum.Blood };

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void WithExpression_ChangesPropertyAndKeepsOthers()
    {
        var original = Create(@"C:\addons\a", "addon.json", "test");
        var modified = original with { GridHash = 42 };

        Assert.Equal(42, modified.GridHash);
        Assert.Equal(original.PreviewHash, modified.PreviewHash);
        Assert.Equal(original.FileInfo, modified.FileInfo);
        Assert.Equal(original.Manifest, modified.Manifest);
    }
}
