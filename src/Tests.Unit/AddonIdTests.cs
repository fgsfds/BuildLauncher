using Core.All;

namespace Tests.Unit;

public sealed class AddonIdTests
{
    [Fact]
    public void Constructor_SetsId()
    {
        var id = new AddonId("foo");
        Assert.Equal("foo", id.Id);
    }

    [Fact]
    public void Constructor_WithoutVersion_SetsVersionToNull()
    {
        var id = new AddonId("foo");
        Assert.Null(id.Version);
    }

    [Fact]
    public void Constructor_WithVersion_SetsVersion()
    {
        var id = new AddonId("foo", "1.0");
        Assert.Equal("1.0", id.Version);
    }

    [Fact]
    public void Constructor_WithNullVersion_SetsVersionToNull()
    {
        var id = new AddonId("foo", null);
        Assert.Null(id.Version);
    }

    [Theory]
    [InlineData("foo", "1.0", "foo", "1.0")]
    [InlineData("foo", null, "foo", null)]
    [InlineData("foo", "1.0", "foo", null)]
    [InlineData("foo", null, "foo", "1.0")]
    [InlineData("FOO", "1.0", "foo", "1.0")]
    [InlineData("foo", "1.0", "FOO", "1.0")]
    public void Equals_ReturnsTrue(string id1, string? version1, string id2, string? version2)
    {
        var a = new AddonId(id1, version1);
        var b = new AddonId(id2, version2);

        Assert.True(a.Equals(b));
        Assert.True(a.Equals((object?)b));
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Theory]
    [InlineData("foo", "1.0", "bar", "1.0")]
    [InlineData("foo", "1.0", "foo", "1.1")]
    [InlineData("foo", "1.10", "foo", "1.9")]
    [InlineData("foo", "1.0-a1", "foo", "1.0-a2")]
    [InlineData("foo", "p1", "foo", "p2")]
    public void Equals_ReturnsFalse(string id1, string? version1, string id2, string? version2)
    {
        var a = new AddonId(id1, version1);
        var b = new AddonId(id2, version2);

        Assert.False(a.Equals(b));
        Assert.False(a.Equals((object?)b));
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        var a = new AddonId("foo");
        var b = a;

        Assert.True(a.Equals(b));
        Assert.True(a == b);
    }

    [Fact]
    public void GetHashCode_EqualObjects_ReturnsSameHash()
    {
        var a = new AddonId("foo", "1.0");
        var b = new AddonId("foo", "1.0");

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_CaseInsensitiveId_ReturnsSameHash()
    {
        var a = new AddonId("FOO", "1.0");
        var b = new AddonId("foo", "1.0");

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_NullVersion_IsConsistent()
    {
        var a = new AddonId("foo", null);
        var b = new AddonId("foo", null);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentValues_ReturnsDifferentHash()
    {
        var a = new AddonId("foo", "1.0");
        var b = new AddonId("bar", "1.0");

        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentVersions_ReturnsDifferentHash()
    {
        var a = new AddonId("foo", "1.0");
        var b = new AddonId("foo", "2.0");

        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }
}
