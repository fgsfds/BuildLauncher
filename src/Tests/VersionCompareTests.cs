using Common.Helpers;

namespace Tests;

public sealed class VersionCompareTests
{
    [Theory]
    [InlineData("1.1", "==1.1")]
    [InlineData("1.1", ">=1.1")]
    [InlineData("1.1", ">=1.0")]
    [InlineData("1.1", "<=1.1")]
    [InlineData("1.1", "<=1.2")]
    [InlineData("1.1", ">1.0")]
    [InlineData("1.1", "<1.3")]
    [InlineData(null, "==1")]
    public void CompareTestTrue(string? v1, string v2)
    {
        var result = VersionComparer.Compare(v1, v2);

        Assert.True(result);
    }

    [Theory]
    [InlineData("1.1", "==1.2")]
    [InlineData("1.1", ">=1.2")]
    [InlineData("1.1", "<=1.0")]
    [InlineData("1.1", ">1.1")]
    [InlineData("1.1", "<1.1")]
    public void CompareTestFalse(string v1, string v2)
    {
        var result = VersionComparer.Compare(v1, v2);

        Assert.False(result);
    }

    [Theory]
    [InlineData("1.1-a1", "==1.1-a1")]
    [InlineData("1.1-a1", ">=1.1-a1")]
    [InlineData("1.1-a1", ">=1.1-a0")]
    [InlineData("1.1-a1", "<=1.1-a1")]
    [InlineData("1.1-a1", "<=1.1-a2")]
    [InlineData("1.1-a1", ">1.1-a0")]
    [InlineData("1.1-a1", "<1.1-a3")]
    public void CompareTest2(string v1, string v2)
    {
        var result = VersionComparer.Compare(v1, v2);

        Assert.True(result);
    }

    [Theory]
    [InlineData(null, null, "==")]
    public void CompareTest3(string? v1, string? v2, string c)
    {
        var result = VersionComparer.Compare(v1, v2, c);

        Assert.True(result);
    }
}
