using Core.All;

namespace Tests.Unit;

/// <summary>
///     Tests for the <see cref="AddonId" /> record.
/// </summary>
public sealed class AddonIdTests
{
    /// <summary>
    ///     Tests that the constructor sets the Id property.
    /// </summary>
    [Fact]
    public void Constructor_SetsId()
    {
        var id = new AddonId("foo");
        Assert.Equal("foo", id.Id);
    }

    /// <summary>
    ///     Tests that the constructor sets Version to null when no version is provided.
    /// </summary>
    [Fact]
    public void Constructor_WithoutVersion_SetsVersionToNull()
    {
        var id = new AddonId("foo");
        Assert.Null(id.Version);
    }

    /// <summary>
    ///     Tests that the constructor sets the version when provided.
    /// </summary>
    [Fact]
    public void Constructor_WithVersion_SetsVersion()
    {
        var id = new AddonId("foo", "1.0");
        Assert.Equal("1.0", id.Version);
    }

    /// <summary>
    ///     Tests that the constructor sets Version to null when null is passed.
    /// </summary>
    [Fact]
    public void Constructor_WithNullVersion_SetsVersionToNull()
    {
        var id = new AddonId("foo", null);
        Assert.Null(id.Version);
    }

    /// <summary>
    ///     Tests that <see cref="AddonId.Equals(AddonId)" /> returns true for equal identifiers.
    /// </summary>
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

    /// <summary>
    ///     Tests that <see cref="AddonId.Equals(AddonId)" /> returns false for different identifiers.
    /// </summary>
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

    /// <summary>
    ///     Tests that <see cref="AddonId.Equals(AddonId)" /> returns true for the same reference.
    /// </summary>
    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        var a = new AddonId("foo");
        var b = a;

        Assert.True(a.Equals(b));
        Assert.True(a == b);
    }

    /// <summary>
    ///     Tests that <see cref="AddonId.GetHashCode" /> returns the same hash for equal objects.
    /// </summary>
    [Fact]
    public void GetHashCode_EqualObjects_ReturnsSameHash()
    {
        var a = new AddonId("foo", "1.0");
        var b = new AddonId("foo", "1.0");

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    /// <summary>
    ///     Tests that <see cref="AddonId.GetHashCode" /> is case-insensitive for the Id.
    /// </summary>
    [Fact]
    public void GetHashCode_CaseInsensitiveId_ReturnsSameHash()
    {
        var a = new AddonId("FOO", "1.0");
        var b = new AddonId("foo", "1.0");

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    /// <summary>
    ///     Tests that <see cref="AddonId.GetHashCode" /> is consistent with null versions.
    /// </summary>
    [Fact]
    public void GetHashCode_NullVersion_IsConsistent()
    {
        var a = new AddonId("foo", null);
        var b = new AddonId("foo", null);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    /// <summary>
    ///     Tests that <see cref="AddonId.GetHashCode" /> returns different hashes for different Ids.
    /// </summary>
    [Fact]
    public void GetHashCode_DifferentValues_ReturnsDifferentHash()
    {
        var a = new AddonId("foo", "1.0");
        var b = new AddonId("bar", "1.0");

        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }

    /// <summary>
    ///     Tests that <see cref="AddonId.GetHashCode" /> returns different hashes for different versions.
    /// </summary>
    [Fact]
    public void GetHashCode_DifferentVersions_ReturnsDifferentHash()
    {
        var a = new AddonId("foo", "1.0");
        var b = new AddonId("foo", "2.0");

        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }
}
