using Core.All.Enums;
using Core.Client.Interfaces;
using Games.Providers;
using Moq;

namespace Tests.Unit;

public sealed class InstalledGamesProviderTests
{
    private readonly InstalledGamesProvider _provider;
    private readonly Mock<IConfigProvider> _configMock;

    public InstalledGamesProviderTests()
    {
        _configMock = new Mock<IConfigProvider>();
        _configMock.Setup(x => x.PathDuke3D).Returns((string?)null);
        _configMock.Setup(x => x.PathBlood).Returns((string?)null);
        _configMock.Setup(x => x.PathWang).Returns((string?)null);
        _configMock.Setup(x => x.PathFury).Returns((string?)null);
        _configMock.Setup(x => x.PathRedneck).Returns((string?)null);
        _configMock.Setup(x => x.PathRidesAgain).Returns((string?)null);
        _configMock.Setup(x => x.PathSlave).Returns((string?)null);
        _configMock.Setup(x => x.PathNam).Returns((string?)null);
        _configMock.Setup(x => x.PathWW2GI).Returns((string?)null);
        _configMock.Setup(x => x.PathWitchaven).Returns((string?)null);
        _configMock.Setup(x => x.PathWitchaven2).Returns((string?)null);
        _configMock.Setup(x => x.PathTekWar).Returns((string?)null);
        _configMock.Setup(x => x.PathDukeWT).Returns((string?)null);
        _configMock.Setup(x => x.PathDuke64).Returns((string?)null);
        _configMock.Setup(x => x.PathDukeZH).Returns((string?)null);

        _provider = new InstalledGamesProvider(_configMock.Object);
    }

    [Fact]
    public void GetGame_Duke3D_ReturnsDukeGame()
    {
        var game = _provider.GetGame(GameEnum.Duke3D);
        Assert.NotNull(game);
        Assert.Equal(GameEnum.Duke3D, game.GameEnum);
    }

    [Fact]
    public void GetGame_Blood_ReturnsBloodGame()
    {
        var game = _provider.GetGame(GameEnum.Blood);
        Assert.NotNull(game);
        Assert.Equal(GameEnum.Blood, game.GameEnum);
    }

    [Fact]
    public void GetGame_Wang_ReturnsWangGame()
    {
        var game = _provider.GetGame(GameEnum.Wang);
        Assert.NotNull(game);
        Assert.Equal(GameEnum.Wang, game.GameEnum);
    }

    [Fact]
    public void GetGames_ReturnsAllGames()
    {
        var games = _provider.GetGames();
        Assert.NotEmpty(games);
        Assert.Contains(games, g => g.GameEnum == GameEnum.Duke3D);
        Assert.Contains(games, g => g.GameEnum == GameEnum.Blood);
    }

    [Fact]
    public void IsBloodInstalled_WhenNoPath_ReturnsFalse()
    {
        Assert.False((bool)_provider.IsBloodInstalled);
    }

    [Fact]
    public void IsDukeInstalled_WhenNoPath_ReturnsFalse()
    {
        Assert.False((bool)_provider.IsDukeInstalled);
    }

    [Fact]
    public void GameChangedEvent_FiresWhenPathChanges()
    {
        GameEnum? received = null;
        _provider.GameChangedEvent += g => received = g;

        _configMock.Raise(x => x.ParameterChangedEvent += null, "PathDuke3D");

        Assert.NotNull(received);
        Assert.Equal(GameEnum.Duke3D, received);
    }
}
