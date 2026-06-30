using Addons.Addons;
using Addons.Providers;
using Core.All.Enums;

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
    private readonly string _tempFolder;

    public GrpInfoProviderTests()
    {
        _grpInfoFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.grpinfo");
        File.WriteAllText(_grpInfoFilePath, GrpInfo);

        _tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempFolder);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (File.Exists(_grpInfoFilePath))
        {
            File.Delete(_grpInfoFilePath);
        }

        if (Directory.Exists(_tempFolder))
        {
            Directory.Delete(_tempFolder, true);
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

    [Fact]
    public void TryGetAddonsFromGrpInfo_GrpsMatchGrpInfo_ReturnsTrueWithDukeCampaigns()
    {
        var grpInfoPath = Path.Combine(_tempFolder, "test.grpinfo");

        File.WriteAllText(grpInfoPath, """
                          grpinfo
                          {
                              name       "Test Campaign"
                              scriptname "scripts/test.con"
                              defname    "test.def"
                              size       1000
                          }

                          grpinfo
                          {
                              name       "Second Campaign"
                              size       2000
                          }
                          """);

        var grp1 = Path.Combine(_tempFolder, "test.grp");
        var grp2 = Path.Combine(_tempFolder, "second.grp");
        File.WriteAllBytes(grp1, new byte[1000]);
        File.WriteAllBytes(grp2, new byte[2000]);

        var result = GrpInfoProvider.TryGetAddonsFromGrpInfo(grpInfoPath, out var addons);

        Assert.True(result);
        Assert.NotNull(addons);
        Assert.Equal(2, addons.Count);

        var first = addons.First(a => a.Title == "Test Campaign");
        Assert.Equal("test_campaign", first.AddonId.Id);
        Assert.Null(first.AddonId.Version);
        Assert.Equal(AddonTypeEnum.TC, first.Type);
        Assert.Equal(GameEnum.Duke3D, first.SupportedGame.GameEnum);
        Assert.Equal("scripts/test.con", ((DukeCampaign)first).MainCon);
        Assert.NotNull(((DukeCampaign)first).AdditionalDefs);
        Assert.Contains("test.def", ((DukeCampaign)first).AdditionalDefs!);
        Assert.Contains(FeatureEnum.EDuke32_CON, first.RequiredFeatures!);

        var second = addons.First(a => a.Title == "Second Campaign");
        Assert.Equal("second_campaign", second.AddonId.Id);
        Assert.Null(((DukeCampaign)second).MainCon);
        Assert.Null(((DukeCampaign)second).AdditionalDefs);
    }

    [Fact]
    public void TryGetAddonsFromGrpInfo_NoGrpFiles_ReturnsFalse()
    {
        var grpInfoPath = Path.Combine(_tempFolder, "empty.grpinfo");

        File.WriteAllText(grpInfoPath, """
                          grpinfo
                          {
                              name       "Test"
                              size       1000
                          }
                          """);

        var result = GrpInfoProvider.TryGetAddonsFromGrpInfo(grpInfoPath, out var addons);

        Assert.False(result);
        Assert.Null(addons);
    }

    [Fact]
    public void TryGetAddonsFromGrpInfo_GrpSizeNotInGrpInfo_Skipped()
    {
        var grpInfoPath = Path.Combine(_tempFolder, "partial.grpinfo");

        File.WriteAllText(grpInfoPath, """
                          grpinfo
                          {
                              name       "Matched"
                              size       500
                          }
                          """);

        var matchedGrp = Path.Combine(_tempFolder, "matched.grp");
        var unmatchedGrp = Path.Combine(_tempFolder, "unmatched.grp");
        File.WriteAllBytes(matchedGrp, new byte[500]);
        File.WriteAllBytes(unmatchedGrp, new byte[999]);

        var result = GrpInfoProvider.TryGetAddonsFromGrpInfo(grpInfoPath, out var addons);

        Assert.True(result);
        Assert.NotNull(addons);
        Assert.Single(addons);
        Assert.Equal("Matched", addons[0].Title);
    }

    [Fact]
    public void TryGetAddonsFromGrpInfo_EntryMissingName_Skipped()
    {
        var grpInfoPath = Path.Combine(_tempFolder, "noname.grpinfo");

        File.WriteAllText(grpInfoPath, """
                          grpinfo
                          {
                              // no name
                              size       100
                          }

                          grpinfo
                          {
                              name       "Has Name"
                              size       200
                          }
                          """);

        File.WriteAllBytes(Path.Combine(_tempFolder, "a.grp"), new byte[100]);
        File.WriteAllBytes(Path.Combine(_tempFolder, "b.grp"), new byte[200]);

        var result = GrpInfoProvider.TryGetAddonsFromGrpInfo(grpInfoPath, out var addons);

        Assert.True(result);
        Assert.NotNull(addons);
        Assert.Single(addons);
        Assert.Equal("Has Name", addons[0].Title);
    }
}
