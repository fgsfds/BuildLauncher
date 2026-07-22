using Core.All.Enums;
using Core.All.Enums.Versions;
using Core.Client.Interfaces;
using Games.Providers;
using Moq;

namespace Tests.Unit;

public sealed class GamesPathsProviderTests
{
    [Fact]
    public void Constructor_DoesNotThrow()
    {
        var configMock = new Mock<IConfigProvider>();

        var provider = new GamesPathsProvider(configMock.Object);
    }

    [Fact]
    public void GetPath_SupportedGame_ReturnsNullOrString()
    {
        var configMock = new Mock<IConfigProvider>();
        var provider = new GamesPathsProvider(configMock.Object);

        foreach (var game in new[]
        {
            GameEnum.Duke3D, GameEnum.Wang, GameEnum.Blood, GameEnum.Fury,
            GameEnum.Slave, GameEnum.Redneck, GameEnum.RidesAgain, GameEnum.NAM,
            GameEnum.WW2GI, GameEnum.Witchaven, GameEnum.Witchaven2, GameEnum.TekWar
        })
        {
            var path = provider.GetPath(game);
            Assert.True(path is null || Directory.Exists(path));
        }
    }

    [Fact]
    public void GetPath_UnsupportedGame_ThrowsNotSupportedException()
    {
        var configMock = new Mock<IConfigProvider>();
        var provider = new GamesPathsProvider(configMock.Object);

        Assert.Throws<NotSupportedException>(() => provider.GetPath(GameEnum.Standalone));
        Assert.Throws<NotSupportedException>(() => provider.GetPath(GameEnum.Duke64));
        Assert.Throws<NotSupportedException>(() => provider.GetPath(GameEnum.DukeZeroHour));
    }

    [Fact]
    public void GetPath_DukeVersion_Supported_ReturnsNullOrString()
    {
        var configMock = new Mock<IConfigProvider>();
        var provider = new GamesPathsProvider(configMock.Object);

        foreach (var version in new[]
        {
            DukeVersionEnum.Duke3D_13D,
            DukeVersionEnum.Duke3D_Atomic,
            DukeVersionEnum.Duke3D_WT
        })
        {
            var path = provider.GetPath(version);
            Assert.True(path is null || Directory.Exists(path));
        }
    }

    [Fact]
    public void GetPath_DukeVersion_Unsupported_ThrowsNotSupportedException()
    {
        var configMock = new Mock<IConfigProvider>();
        var provider = new GamesPathsProvider(configMock.Object);

        Assert.Throws<NotSupportedException>(() => provider.GetPath((DukeVersionEnum)(-1)));
    }
}
