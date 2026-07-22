using Core.All;

namespace Tests.Unit;

public sealed class ResultTests
{
    [Fact]
    public void Constructor_SetsMessage()
    {
        var r = new Result(ResultEnum.Error, "something went wrong");
        Assert.Equal("something went wrong", r.Message);
    }

    [Fact]
    public void Constructor_WithSuccess_IsSuccessReturnsTrue()
    {
        var r = new Result(ResultEnum.Success, "ok");
        Assert.True(r.IsSuccess);
    }

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

    [Fact]
    public void Default_ResultEnumIsSuccess_IsSuccessIsTrue()
    {
        var r = default(Result);
        Assert.True(r.IsSuccess);
        Assert.Null(r.Message);
    }

    [Fact]
    public void Message_CanBeNull()
    {
        var r = new Result(ResultEnum.Success, null!);
        Assert.Null(r.Message);
        Assert.True(r.IsSuccess);
    }

    [Theory]
    [InlineData(ResultEnum.Success)]
    [InlineData(ResultEnum.HashError)]
    [InlineData(ResultEnum.NotFound)]
    [InlineData(ResultEnum.ConnectionError)]
    [InlineData(ResultEnum.FileAccessError)]
    [InlineData(ResultEnum.Cancelled)]
    [InlineData(ResultEnum.Error)]
    public void ResultEnum_Property_ReturnsValuePassedToConstructor(ResultEnum expected)
    {
        var r = new Result(expected, "msg");
        Assert.Equal(expected, r.ResultEnum);
    }

    [Fact]
    public void Default_ResultEnumIsSuccess()
    {
        var r = default(Result);
        Assert.Equal(ResultEnum.Success, r.ResultEnum);
    }
}


public sealed class ResultGenericTests
{
    [Fact]
    public void Constructor_SetsResultObject()
    {
        var r = new Result<string>(ResultEnum.Success, "data", "ok");
        Assert.Equal("data", r.ResultObject);
    }

    [Fact]
    public void Constructor_WithValueTypeResultObject()
    {
        var r = new Result<int>(ResultEnum.Success, 42, "ok");
        Assert.Equal(42, r.ResultObject);
    }

    [Fact]
    public void MemberNotNullWhen_NonNullResultObject_AfterSuccessCheck()
    {
        var r = new Result<string>(ResultEnum.Success, "data", "ok");
        Assert.NotNull(r.ResultObject);
    }

    [Fact]
    public void Default_ResultEnumIsSuccess_IsSuccessIsTrue()
    {
        var r = default(Result<string>);
        Assert.True(r.IsSuccess);
        Assert.Null(r.ResultObject);
        Assert.Null(r.Message);
    }

    [Fact]
    public void NullResultObject_OnSuccess_IsAllowed()
    {
        var r = new Result<string>(ResultEnum.Success, null, "ok");
        Assert.Null(r.ResultObject);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void NonSuccess_WithNonNullResultObject_HasResultObjectButNotSuccess()
    {
        var r = new Result<string>(ResultEnum.NotFound, "data", "not found");
        Assert.Equal("data", r.ResultObject);
        Assert.False(r.IsSuccess);
    }
}
