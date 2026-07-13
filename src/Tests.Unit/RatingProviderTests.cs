using Core.Client.Interfaces;
using Core.Client.Providers;
using Moq;

namespace Tests.Unit;

public sealed class RatingProviderTests
{
    [Fact]
    public void GetRating_NullCache_ReturnsNull()
    {
        var apiMock = new Mock<IApiInterface>();
        apiMock.Setup(x => x.GetRatingsAsync()).ReturnsAsync((Dictionary<string, decimal>?)null);

        var configMock = new Mock<IConfigProvider>();

        var provider = new RatingProvider(apiMock.Object, configMock.Object);
        var rating = provider.GetRating("addon1");

        Assert.Null(rating);
    }

    [Fact]
    public async Task GetRating_AfterCachePopulated_ReturnsValue()
    {
        var apiMock = new Mock<IApiInterface>();
        apiMock.Setup(x => x.GetRatingsAsync()).ReturnsAsync(new Dictionary<string, decimal>
        {
            ["addon1"] = 4.5m
        });

        var configMock = new Mock<IConfigProvider>();

        var provider = new RatingProvider(apiMock.Object, configMock.Object);

        await Task.Delay(50);

        var rating = provider.GetRating("addon1");
        Assert.NotNull(rating);
        Assert.Equal(4.5m, rating);
    }

    [Fact]
    public void GetRating_NotInCache_ReturnsNull()
    {
        var apiMock = new Mock<IApiInterface>();
        apiMock.Setup(x => x.GetRatingsAsync()).ReturnsAsync(new Dictionary<string, decimal>
        {
            ["addon1"] = 4.5m
        });

        var configMock = new Mock<IConfigProvider>();

        var provider = new RatingProvider(apiMock.Object, configMock.Object);

        Thread.Sleep(100);

        var rating = provider.GetRating("unknown");
        Assert.Null(rating);
    }
}
