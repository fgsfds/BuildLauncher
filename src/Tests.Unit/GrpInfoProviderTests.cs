using Addons.Providers;

namespace Tests.Unit;

public sealed class GrpInfoProviderTests : IDisposable
{
    private const string GrpInfo = """
        // EDUKE32 ADDON COMPILATION v4.0 (by NightFright)
        // ATOMIC EDITION

        grpinfo
        {
            name       "1999/2000 TC"
            scriptname "scripts/2000tc.con"
            size       17147390
            crc        0x23F56C4E
            flags      16
            dependency DUKE15_CRC
        }

        grpinfo
        {
            name       "25th Century Duke"
            scriptname "scripts/25th_century.con"
            defname    "add.def"
            size       1385747
            crc        0x42836014
            flags      16
        }

        grpinfo
        {
            name       "A.Dream Trilogy"
            size       28245809
            crc        0x01987700
            flags      16
        }
        """;

    private readonly string _grpInfoFilePath;

    public GrpInfoProviderTests()
    {
        _grpInfoFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.grpinfo");
        File.WriteAllText(_grpInfoFilePath, GrpInfo);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (File.Exists(_grpInfoFilePath))
        {
            File.Delete(_grpInfoFilePath);
        }
    }

    [Fact]
    public void Parse_ReturnTrue()
    {
        var result = GrpInfoProvider.Parse(_grpInfoFilePath);

        Assert.Equal(3, result.Count);

        var entry1 = result.First(x => x.Name == "1999/2000 TC");
        Assert.Equal("scripts/2000tc.con", entry1.MainCon);
        Assert.Null(entry1.AddDef);
        Assert.Equal(17147390, entry1.Size);

        var entry2 = result.First(x => x.Name == "25th Century Duke");
        Assert.Equal("scripts/25th_century.con", entry2.MainCon);
        Assert.Equal("add.def", entry2.AddDef);
        Assert.Equal(1385747, entry2.Size);

        var entry3 = result.First(x => x.Name == "A.Dream Trilogy");
        Assert.Null(entry3.MainCon);
        Assert.Null(entry3.AddDef);
        Assert.Equal(28245809, entry3.Size);
    }

    [Fact]
    public void Parse_EmptyFile_ReturnsEmptyList()
    {
        var emptyPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.grpinfo");
        File.WriteAllText(emptyPath, "");

        try
        {
            var result = GrpInfoProvider.Parse(emptyPath);

            Assert.Empty(result);
        }
        finally
        {
            File.Delete(emptyPath);
        }
    }

    [Fact]
    public void Parse_OnlyCommentsAndWhitespace_ReturnsEmptyList()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.grpinfo");
        File.WriteAllText(path, """
            // just a comment

            // another comment
            """);

        try
        {
            var result = GrpInfoProvider.Parse(path);

            Assert.Empty(result);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Parse_EntriesMissingNameOrSize_Skipped()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.grpinfo");
        File.WriteAllText(path, """
            grpinfo
            {
                name       "Valid Entry"
                size       100
            }

            grpinfo
            {
                // no name and no size
            }

            grpinfo
            {
                name       "No Size Entry"
                // size missing
            }
            """);

        try
        {
            var result = GrpInfoProvider.Parse(path);

            Assert.Single(result);
            Assert.Equal("Valid Entry", result[0].Name);
            Assert.Equal(100, result[0].Size);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Parse_SingleEntry_ReturnsOneEntry()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.grpinfo");
        File.WriteAllText(path, """
            grpinfo
            {
                name       "Single Addon"
                scriptname "scripts/main.con"
                defname    "add.def"
                size       5000
                crc        0x12345678
                flags      16
            }
            """);

        try
        {
            var result = GrpInfoProvider.Parse(path);

            var entry = Assert.Single(result);
            Assert.Equal("Single Addon", entry.Name);
            Assert.Equal("scripts/main.con", entry.MainCon);
            Assert.Equal("add.def", entry.AddDef);
            Assert.Equal(5000, entry.Size);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
