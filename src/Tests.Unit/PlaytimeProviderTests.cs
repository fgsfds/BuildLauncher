using Core.Client.Interfaces;
using Core.Client.Providers;
using Moq;

namespace Tests.Unit;

public sealed class PlaytimeProviderTests
{
    [Fact]
    public void GetTime_PreExistingTime_ReturnsCorrectValue()
    {
        var configMock = new Mock<IConfigProvider>();
        configMock.Setup(x => x.Playtimes).Returns(new Dictionary<string, TimeSpan>
        {
            ["addon1"] = TimeSpan.FromHours(5)
        });

        var provider = new PlaytimeProvider(configMock.Object);
        var time = provider.GetTime("addon1");

        Assert.NotNull(time);
        Assert.Equal(TimeSpan.FromHours(5), time);
    }

    [Fact]
    public void GetTime_UnknownAddon_ReturnsNull()
    {
        var configMock = new Mock<IConfigProvider>();
        configMock.Setup(x => x.Playtimes).Returns(new Dictionary<string, TimeSpan>());

        var provider = new PlaytimeProvider(configMock.Object);
        var time = provider.GetTime("nonexistent");

        Assert.Null(time);
    }

    [Fact]
    public void AddTime_NewEntry_StoresCorrectly()
    {
        var configMock = new Mock<IConfigProvider>();
        configMock.Setup(x => x.Playtimes).Returns(new Dictionary<string, TimeSpan>());

        var provider = new PlaytimeProvider(configMock.Object);
        provider.AddTime("addon1", TimeSpan.FromMinutes(30));

        var time = provider.GetTime("addon1");
        Assert.NotNull(time);
        Assert.Equal(TimeSpan.FromMinutes(30), time);
        configMock.Verify(x => x.AddPlaytime("addon1", TimeSpan.FromMinutes(30)), Times.Once);
    }

    [Fact]
    public void AddTime_ExistingEntry_Accumulates()
    {
        var configMock = new Mock<IConfigProvider>();
        configMock.Setup(x => x.Playtimes).Returns(new Dictionary<string, TimeSpan>
        {
            ["addon1"] = TimeSpan.FromHours(1)
        });

        var provider = new PlaytimeProvider(configMock.Object);
        provider.AddTime("addon1", TimeSpan.FromMinutes(30));

        var time = provider.GetTime("addon1");
        Assert.NotNull(time);
        Assert.Equal(TimeSpan.FromHours(1.5), time);
    }

    [Fact]
    public void AddTime_ZeroTime_StoresCorrectly()
    {
        var configMock = new Mock<IConfigProvider>();
        configMock.Setup(x => x.Playtimes).Returns(new Dictionary<string, TimeSpan>());

        var provider = new PlaytimeProvider(configMock.Object);
        provider.AddTime("addon1", TimeSpan.Zero);

        var time = provider.GetTime("addon1");
        Assert.NotNull(time);
        Assert.Equal(TimeSpan.Zero, time);
    }
}
