using Core.All.Enums;
using Core.Client.Api;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests.Unit;

public sealed class OfflineApiInterfaceTests
{
    private readonly OfflineApiInterface _api = new(NullLogger<OfflineApiInterface>.Instance);

    [Fact]
    public async Task GetLatestAppReleaseAsync_ReturnsNull()
    {
        var result = await _api.GetLatestAppReleaseAsync();
        Assert.Null(result);
    }

    [Fact]
    public async Task GetLatestPortReleaseAsync_ReturnsNull()
    {
        var result = await _api.GetLatestPortReleaseAsync(PortEnum.EDuke32);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetLatestToolReleaseAsync_ReturnsNull()
    {
        var result = await _api.GetLatestToolReleaseAsync(ToolEnum.Mapster32);
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAddonToDatabaseAsync_ReturnsFalse()
    {
        var result = await _api.AddAddonToDatabaseAsync(null!, null!);
        Assert.False(result);
    }

    [Fact]
    public async Task GetRatingsAsync_ReturnsNull()
    {
        var result = await _api.GetRatingsAsync();
        Assert.Null(result);
    }

    [Fact]
    public async Task ChangeScoreAsync_ReturnsNull()
    {
        var result = await _api.ChangeScoreAsync("test", 5, true);
        Assert.Null(result);
    }

    [Fact]
    public async Task IncreaseNumberOfInstallsAsync_ReturnsFalse()
    {
        var result = await _api.IncreaseNumberOfInstallsAsync("test");
        Assert.False(result);
    }
}
