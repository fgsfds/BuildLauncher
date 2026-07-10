using Core.All.Helpers;

namespace Tests.Unit;

public sealed class ExtensionsTests
{
    [Theory]
    [InlineData(0, "0B")]
    [InlineData(1, "1B")]
    [InlineData(500, "500B")]
    [InlineData(999, "999B")]
    public void ToSizeString_Bytes(long bytes, string expected)
    {
        Assert.Equal(expected, bytes.ToSizeString());
    }

    [Theory]
    [InlineData(1000, "1KB")]
    [InlineData(1500, "1KB")]
    [InlineData(999000, "999KB")]
    [InlineData(999999, "999KB")]
    public void ToSizeString_Kilobytes(long bytes, string expected)
    {
        Assert.Equal(expected, bytes.ToSizeString());
    }

    [Theory]
    [InlineData(1_000_000, "1MB")]
    [InlineData(1_500_000, "2MB")]
    [InlineData(15_000_000, "15MB")]
    [InlineData(999_000_000, "999MB")]
    public void ToSizeString_Megabytes(long bytes, string expected)
    {
        Assert.Equal(expected, bytes.ToSizeString());
    }

    [Theory]
    [InlineData(1_000_000_000, "1GB")]
    [InlineData(1_500_000_000, "1.5GB")]
    [InlineData(2_000_000_000, "2GB")]
    [InlineData(10_000_000_000, "10GB")]
    [InlineData(15_700_000_000, "15.7GB")]
    [InlineData(100_000_000_000, "100GB")]
    public void ToSizeString_Gigabytes(long bytes, string expected)
    {
        Assert.Equal(expected, bytes.ToSizeString());
    }

    [Fact]
    public void ToTimeString_NeverPlayed()
    {
        Assert.Equal("never played", TimeSpan.Zero.ToTimeString());
        Assert.Equal("never played", TimeSpan.FromMilliseconds(500).ToTimeString());
    }

    [Theory]
    [InlineData(1, " 1 second")]
    [InlineData(2, " 2 seconds")]
    [InlineData(3, " 3 seconds")]
    [InlineData(30, " 30 seconds")]
    [InlineData(59, " 59 seconds")]
    public void ToTimeString_Seconds(int seconds, string expected)
    {
        Assert.Equal(expected, TimeSpan.FromSeconds(seconds).ToTimeString());
    }

    [Theory]
    [InlineData(60, " 1 minute")]
    [InlineData(120, " 2 minutes")]
    [InlineData(900, " 15 minutes")]
    [InlineData(3540, " 59 minutes")]
    [InlineData(90, " 1 minute")]
    public void ToTimeString_Minutes(int totalSeconds, string expected)
    {
        Assert.Equal(expected, TimeSpan.FromSeconds(totalSeconds).ToTimeString());
    }

    [Theory]
    [InlineData(3600, " 1 hour")]
    [InlineData(5400, " 1 hour 30 minutes")]
    [InlineData(7200, " 2 hours")]
    [InlineData(8100, " 2 hours 15 minutes")]
    [InlineData(86400, " 24 hours")]
    public void ToTimeString_Hours(int totalSeconds, string expected)
    {
        Assert.Equal(expected, TimeSpan.FromSeconds(totalSeconds).ToTimeString());
    }

    [Fact]
    public void ToTimeString_HourAndMinute_NoDuplicateSpaces()
    {
        var result = TimeSpan.FromHours(1).ToTimeString();
        Assert.DoesNotContain("  ", result.TrimStart());
    }

    [Fact]
    public void AddOrReplace_NewKey_AddsToDictionary()
    {
        Dictionary<string, int> dict = [];
        dict.AddOrReplace("key1", 42);

        Assert.Single(dict);
        Assert.Equal(42, dict["key1"]);
    }

    [Fact]
    public void AddOrReplace_ExistingKey_ReplacesValue()
    {
        Dictionary<string, int> dict = new()
        {
            ["key1"] = 10
        };

        dict.AddOrReplace("key1", 42);

        Assert.Single(dict);
        Assert.Equal(42, dict["key1"]);
    }

    [Fact]
    public void AddOrReplace_ExistingKey_DoesNotIncreaseCount()
    {
        Dictionary<string, int> dict = new()
        {
            ["key1"] = 10,
            ["key2"] = 20
        };

        dict.AddOrReplace("key1", 42);

        Assert.Equal(2, dict.Count);
    }

    [Fact]
    public void AddRange_DictionaryOverload_MergesAllKeys()
    {
        Dictionary<string, int> target = new()
        {
            ["a"] = 1
        };

        Dictionary<string, int> source = new()
        {
            ["b"] = 2,
            ["c"] = 3
        };

        target.AddRange(source);

        Assert.Equal(3, target.Count);
        Assert.Equal(1, target["a"]);
        Assert.Equal(2, target["b"]);
        Assert.Equal(3, target["c"]);
    }

    [Fact]
    public void AddRange_DictionaryOverload_OverwritesExisting()
    {
        Dictionary<string, int> target = new()
        {
            ["a"] = 1,
            ["b"] = 2
        };

        Dictionary<string, int> source = new()
        {
            ["b"] = 99,
            ["c"] = 3
        };

        target.AddRange(source);

        Assert.Equal(3, target.Count);
        Assert.Equal(99, target["b"]);
    }

    [Fact]
    public void AddRange_DictionaryOverload_EmptySource_DoesNothing()
    {
        Dictionary<string, int> target = new()
        {
            ["a"] = 1
        };

        target.AddRange(new Dictionary<string, int>());

        Assert.Single(target);
    }

    [Fact]
    public void AddRange_EnumerableOverload_MergesAllKeys()
    {
        Dictionary<string, int> target = new()
        {
            ["a"] = 1
        };

        List<KeyValuePair<string, int>> source =
        [
            new("b", 2),
            new("c", 3)
        ];

        target.AddRange((IEnumerable<KeyValuePair<string, int>>)source);

        Assert.Equal(3, target.Count);
        Assert.Equal(2, target["b"]);
        Assert.Equal(3, target["c"]);
    }

    [Fact]
    public void AddRange_EnumerableOverload_OverwritesExisting()
    {
        Dictionary<string, int> target = new()
        {
            ["a"] = 1,
            ["b"] = 2
        };

        List<KeyValuePair<string, int>> source = [new("b", 99)];

        target.AddRange((IEnumerable<KeyValuePair<string, int>>)source);

        Assert.Equal(2, target.Count);
        Assert.Equal(99, target["b"]);
    }
}
