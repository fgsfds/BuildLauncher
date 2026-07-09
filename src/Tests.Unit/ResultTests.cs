using Core.All;

namespace Tests.Unit;

/// <summary>
///     Tests for the <see cref="Result" /> struct.
/// </summary>
public sealed class ResultTests
{
    /// <summary>
    ///     Tests that the constructor sets the Message property.
    /// </summary>
    [Fact]
    public void Constructor_SetsMessage()
    {
        var r = new Result(ResultEnum.Error, "something went wrong");
        Assert.Equal("something went wrong", r.Message);
    }

    /// <summary>
    ///     Tests that a Success result has IsSuccess equal to true.
    /// </summary>
    [Fact]
    public void Constructor_WithSuccess_IsSuccessReturnsTrue()
    {
        var r = new Result(ResultEnum.Success, "ok");
        Assert.True(r.IsSuccess);
    }

    /// <summary>
    ///     Tests that non-success results have IsSuccess equal to false.
    /// </summary>
    [Theory]
    [InlineData(ResultEnum.HashError)]
    [InlineData(ResultEnum.NotFound)]
    [InlineData(ResultEnum.ConnectionError)]
    [InlineData(ResultEnum.FileAccessError)]
    [InlineData(ResultEnum.Cancelled)]
    [InlineData(ResultEnum.Error)]
    public void Constructor_WithNonSuccess_IsSuccessReturnsFalse(ResultEnum resultEnum)
    {
        var r = new Result(resultEnum, "");
        Assert.False(r.IsSuccess);
    }
}

/// <summary>
///     Tests for the <see cref="Result{T}" /> struct.
/// </summary>
public sealed class ResultGenericTests
{
    /// <summary>
    ///     Tests that the constructor sets the ResultObject property.
    /// </summary>
    [Fact]
    public void Constructor_SetsResultObject()
    {
        var r = new Result<string>(ResultEnum.Success, "data", "ok");
        Assert.Equal("data", r.ResultObject);
    }

    /// <summary>
    ///     Tests that the constructor works with value type result objects.
    /// </summary>
    [Fact]
    public void Constructor_WithValueTypeResultObject()
    {
        var r = new Result<int>(ResultEnum.Success, 42, "ok");
        Assert.Equal(42, r.ResultObject);
    }

    /// <summary>
    ///     Tests that ResultObject is not null after a successful result with data.
    /// </summary>
    [Fact]
    public void MemberNotNullWhen_NonNullResultObject_AfterSuccessCheck()
    {
        var r = new Result<string>(ResultEnum.Success, "data", "ok");
        Assert.NotNull(r.ResultObject);
    }
}
