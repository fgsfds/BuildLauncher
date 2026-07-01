using Core.All.Helpers;

namespace Tests.Unit;

/// <summary>
///     Tests for the <see cref="VersionComparer" /> class.
/// </summary>
public sealed class VersionCompareTests
{
    /// <summary>
    ///     Tests that <see cref="VersionComparer.Compare(string?, string?)" /> returns true for matching comparisons.
    /// </summary>
    [Theory]
    [InlineData("1.1", "==1.1")]
    [InlineData("1.1", ">=1.1")]
    [InlineData("1.1", ">=1.0")]
    [InlineData("1.1", "<=1.1")]
    [InlineData("1.1", "<=1.2")]
    [InlineData("1.1", ">1.0")]
    [InlineData("1.1", "<1.3")]
    [InlineData("1.10", ">1.9")]
    [InlineData("1.10", "==1.10")]
    [InlineData("2.0", ">1.9.9")]
    [InlineData("p2", ">p1")]
    [InlineData("1.1", "<=1.10")]
    [InlineData(null, null)]
    [InlineData(null, "==1")]
    [InlineData(null, ">1")]
    [InlineData(null, ">=1")]
    [InlineData(null, "<1")]
    [InlineData(null, "<=1")]
    [InlineData("1", null)]
    [InlineData("1", "1")]
    public void Compare_ShouldReturnTrue(string? v1, string? v2)
    {
        var result = VersionComparer.Compare(v1, v2);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that <see cref="VersionComparer.Compare(string?, string?)" /> returns false for non-matching comparisons.
    /// </summary>
    [Theory]
    [InlineData("1.1", "==1.2")]
    [InlineData("1.1", ">=1.2")]
    [InlineData("1.1", "<=1.0")]
    [InlineData("1.1", ">1.1")]
    [InlineData("1.1", "<1.1")]
    [InlineData("1.10", "<1.9")]
    [InlineData("1.10", "==1.9")]
    [InlineData("1.10", "<=1.9")]
    [InlineData("1.9", ">=1.10")]
    [InlineData("p2", "<p1")]
    [InlineData("1", "2")]
    public void Compare_ShouldReturnFalse(string? v1, string? v2)
    {
        var result = VersionComparer.Compare(v1, v2);

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that <see cref="VersionComparer.Compare(string?, string?, ComparisonOperatorEnum)" /> returns true for matching comparisons.
    /// </summary>
    [Theory]
    [InlineData("1.1-a1", "1.1-a1", ComparisonOperatorEnum.Equals)]
    [InlineData("1.1-a1", "1.1-a1", ComparisonOperatorEnum.GreaterOrEquals)]
    [InlineData("1.1-a1", "1.1-a0", ComparisonOperatorEnum.GreaterOrEquals)]
    [InlineData("1.1-a1", "1.1-a1", ComparisonOperatorEnum.LessOrEquals)]
    [InlineData("1.1-a1", "1.1-a2", ComparisonOperatorEnum.LessOrEquals)]
    [InlineData("1.1-a1", "1.1-a0", ComparisonOperatorEnum.GreaterThan)]
    [InlineData("1.1-a1", "1.1-a3", ComparisonOperatorEnum.LessThan)]
    [InlineData("1.10-a1", "1.9-a1", ComparisonOperatorEnum.GreaterThan)]
    [InlineData("1.0.0", "1", ComparisonOperatorEnum.Equals)]
    [InlineData("1.1.0", "1.1", ComparisonOperatorEnum.Equals)]
    [InlineData("2.0.0", "1.9.9", ComparisonOperatorEnum.GreaterThan)]
    [InlineData(null, null, ComparisonOperatorEnum.Equals)]
    [InlineData(null, null, ComparisonOperatorEnum.GreaterOrEquals)]
    [InlineData(null, null, ComparisonOperatorEnum.LessOrEquals)]
    [InlineData("p2", "p1", ComparisonOperatorEnum.GreaterThan)]
    [InlineData("1.2", "1.1", ComparisonOperatorEnum.GreaterThan)]
    [InlineData("1.1", "1.2", ComparisonOperatorEnum.LessThan)]
    [InlineData("1.2", "1.1", ComparisonOperatorEnum.GreaterOrEquals)]
    [InlineData("1.1", "1.2", ComparisonOperatorEnum.LessOrEquals)]
    [InlineData("1.1", "1.1", ComparisonOperatorEnum.Equals)]
    [InlineData("p1", "p1", ComparisonOperatorEnum.Equals)]
    [InlineData(null, "1", ComparisonOperatorEnum.Equals)]
    [InlineData(null, "1", ComparisonOperatorEnum.GreaterThan)]
    [InlineData(null, "1", ComparisonOperatorEnum.GreaterOrEquals)]
    [InlineData(null, "1", ComparisonOperatorEnum.LessThan)]
    [InlineData(null, "1", ComparisonOperatorEnum.LessOrEquals)]
    [InlineData("1", null, ComparisonOperatorEnum.Equals)]
    [InlineData("1", null, ComparisonOperatorEnum.GreaterThan)]
    [InlineData("1", null, ComparisonOperatorEnum.GreaterOrEquals)]
    [InlineData("1", null, ComparisonOperatorEnum.LessThan)]
    [InlineData("1", null, ComparisonOperatorEnum.LessOrEquals)]
    [InlineData(null, null, ComparisonOperatorEnum.GreaterThan)]
    [InlineData(null, null, ComparisonOperatorEnum.LessThan)]
    public void Compare_WithOperator_ShouldReturnTrue(string? v1, string? v2, ComparisonOperatorEnum op)
    {
        var result = VersionComparer.Compare(v1, v2, op);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that <see cref="VersionComparer.Compare(string?, string?, ComparisonOperatorEnum)" /> returns false for non-matching comparisons.
    /// </summary>
    [Theory]
    [InlineData("1.1", "1.2", ComparisonOperatorEnum.Equals)]
    [InlineData("1.10", "1.9", ComparisonOperatorEnum.Equals)]
    [InlineData("1.1", "1.1", ComparisonOperatorEnum.GreaterThan)]
    [InlineData("1.1", "1.2", ComparisonOperatorEnum.GreaterThan)]
    [InlineData("1.9", "1.10", ComparisonOperatorEnum.GreaterThan)]
    [InlineData("1.1", "1.0", ComparisonOperatorEnum.LessThan)]
    [InlineData("1.1", "1.1", ComparisonOperatorEnum.LessThan)]
    [InlineData("1.1", "1.2", ComparisonOperatorEnum.GreaterOrEquals)]
    [InlineData("1.1", "1.0", ComparisonOperatorEnum.LessOrEquals)]
    [InlineData("p1", "p2", ComparisonOperatorEnum.GreaterThan)]
    [InlineData("1.10", "1.9", ComparisonOperatorEnum.LessThan)]
    [InlineData("1.10", "1.9", ComparisonOperatorEnum.LessOrEquals)]
    [InlineData("1.9", "1.10", ComparisonOperatorEnum.GreaterOrEquals)]
    [InlineData("p1", "p1", ComparisonOperatorEnum.GreaterThan)]
    [InlineData("p1", "p1", ComparisonOperatorEnum.LessThan)]
    public void Compare_WithOperator_ShouldReturnFalse(string? v1, string? v2, ComparisonOperatorEnum op)
    {
        var result = VersionComparer.Compare(v1, v2, op);

        Assert.False(result);
    }
}
