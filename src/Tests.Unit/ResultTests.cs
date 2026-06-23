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
    public void Default_HasDefaultMessage()
    {
        Result r = default;
        Assert.Null(r.Message);
    }

    [Fact]
    public void Default_IsSuccessIsTrue()
    {
        Result r = default;
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void Constructor_WithCancelled_IsNotSuccess()
    {
        var r = new Result(ResultEnum.Cancelled, "cancelled");
        Assert.False(r.IsSuccess);
    }

    [Fact]
    public void Constructor_WithError_IsNotSuccess()
    {
        var r = new Result(ResultEnum.Error, "error");
        Assert.False(r.IsSuccess);
    }
}

public sealed class ResultGenericTests
{
    [Fact]
    public void Constructor_SetsMessage()
    {
        var r = new Result<string>(ResultEnum.Error, null, "fail");
        Assert.Equal("fail", r.Message);
    }

    [Fact]
    public void Constructor_SetsResultObject()
    {
        var r = new Result<string>(ResultEnum.Success, "data", "ok");
        Assert.Equal("data", r.ResultObject);
    }

    [Fact]
    public void Constructor_WithNullResultObject_SetsToNull()
    {
        var r = new Result<string>(ResultEnum.Success, null, "ok");
        Assert.Null(r.ResultObject);
    }

    [Fact]
    public void Constructor_WithValueTypeResultObject()
    {
        var r = new Result<int>(ResultEnum.Success, 42, "ok");
        Assert.Equal(42, r.ResultObject);
    }

    [Fact]
    public void Constructor_WithSuccess_IsSuccessReturnsTrue()
    {
        var r = new Result<string>(ResultEnum.Success, null, "ok");
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
        var r = new Result<string>(resultEnum, null, "");
        Assert.False(r.IsSuccess);
    }

    [Fact]
    public void Default_HasNullResultObject()
    {
        Result<string> r = default;
        Assert.Null(r.ResultObject);
    }

    [Fact]
    public void Default_MessageIsNull()
    {
        Result<string> r = default;
        Assert.Null(r.Message);
    }

    [Fact]
    public void Default_IsSuccessIsTrue()
    {
        Result<string> r = default;
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void MemberNotNullWhen_NonNullResultObject_AfterSuccessCheck()
    {
        var r = new Result<string>(ResultEnum.Success, "data", "ok");
        Assert.NotNull(r.ResultObject);
    }
}
