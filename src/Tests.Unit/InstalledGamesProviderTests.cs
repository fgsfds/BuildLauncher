using Core.All.Enums;
using Core.Client.Interfaces;
using Games.Games;
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
        Assert.IsType<DukeGame>(game);
        Assert.Equal(GameEnum.Duke3D, game.GameEnum);
    }

    [Fact]
    public void GetGame_Blood_ReturnsBloodGame()
    {
        var game = _provider.GetGame(GameEnum.Blood);
        Assert.NotNull(game);
        Assert.IsType<BloodGame>(game);
        Assert.Equal(GameEnum.Blood, game.GameEnum);
    }

    [Fact]
    public void GetGame_Wang_ReturnsWangGame()
    {
        var game = _provider.GetGame(GameEnum.Wang);
        Assert.NotNull(game);
        Assert.IsType<WangGame>(game);
        Assert.Equal(GameEnum.Wang, game.GameEnum);
    }

    [Fact]
    public void GetGame_Fury_ReturnsFuryGame()
    {
        var game = _provider.GetGame(GameEnum.Fury);
        Assert.NotNull(game);
        Assert.IsType<FuryGame>(game);
        Assert.Equal(GameEnum.Fury, game.GameEnum);
    }

    [Fact]
    public void GetGame_Slave_ReturnsSlaveGame()
    {
        var game = _provider.GetGame(GameEnum.Slave);
        Assert.NotNull(game);
        Assert.IsType<SlaveGame>(game);
        Assert.Equal(GameEnum.Slave, game.GameEnum);
    }

    [Fact]
    public void GetGame_Redneck_ReturnsRedneckGame()
    {
        var game = _provider.GetGame(GameEnum.Redneck);
        Assert.NotNull(game);
        Assert.IsType<RedneckGame>(game);
        Assert.Equal(GameEnum.Redneck, game.GameEnum);
    }

    [Fact]
    public void GetGame_RidesAgain_ReturnsSameRedneckGame()
    {
        var ridesAgain = _provider.GetGame(GameEnum.RidesAgain);
        var redneck = _provider.GetGame(GameEnum.Redneck);
        Assert.NotNull(ridesAgain);
        Assert.Same(redneck, ridesAgain);
    }

    [Fact]
    public void GetGame_NAM_ReturnsNamGame()
    {
        var game = _provider.GetGame(GameEnum.NAM);
        Assert.NotNull(game);
        Assert.IsType<NamGame>(game);
        Assert.Equal(GameEnum.NAM, game.GameEnum);
    }

    [Fact]
    public void GetGame_WW2GI_ReturnsWW2GIGame()
    {
        var game = _provider.GetGame(GameEnum.WW2GI);
        Assert.NotNull(game);
        Assert.IsType<WW2GIGame>(game);
        Assert.Equal(GameEnum.WW2GI, game.GameEnum);
    }

    [Fact]
    public void GetGame_Witchaven_ReturnsWitchavenGame()
    {
        var game = _provider.GetGame(GameEnum.Witchaven);
        Assert.NotNull(game);
        Assert.IsType<WitchavenGame>(game);
        Assert.Equal(GameEnum.Witchaven, game.GameEnum);
    }

    [Fact]
    public void GetGame_Witchaven2_ReturnsSameWitchavenGame()
    {
        var witchaven2 = _provider.GetGame(GameEnum.Witchaven2);
        var witchaven = _provider.GetGame(GameEnum.Witchaven);
        Assert.NotNull(witchaven2);
        Assert.Same(witchaven, witchaven2);
    }

    [Fact]
    public void GetGame_TekWar_ReturnsTekWarGame()
    {
        var game = _provider.GetGame(GameEnum.TekWar);
        Assert.NotNull(game);
        Assert.IsType<TekWarGame>(game);
        Assert.Equal(GameEnum.TekWar, game.GameEnum);
    }

    [Fact]
    public void GetGame_Standalone_ReturnsStandaloneGame()
    {
        var game = _provider.GetGame(GameEnum.Standalone);
        Assert.NotNull(game);
        Assert.IsType<StandaloneGame>(game);
        Assert.Equal(GameEnum.Standalone, game.GameEnum);
    }

    [Fact]
    public void GetGame_UnsupportedEnum_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _provider.GetGame(GameEnum.Duke64));
        Assert.Throws<ArgumentOutOfRangeException>(() => _provider.GetGame(GameEnum.DukeZeroHour));
    }

    [Fact]
    public void GetGames_ReturnsAllGames()
    {
        var games = _provider.GetGames();
        Assert.NotEmpty(games);
        Assert.Contains(games, g => g.GameEnum == GameEnum.Duke3D);
        Assert.Contains(games, g => g.GameEnum == GameEnum.Blood);
        Assert.Contains(games, g => g.GameEnum == GameEnum.Wang);
        Assert.Contains(games, g => g.GameEnum == GameEnum.Fury);
        Assert.Contains(games, g => g.GameEnum == GameEnum.Slave);
        Assert.Contains(games, g => g.GameEnum == GameEnum.Redneck);
        Assert.Contains(games, g => g.GameEnum == GameEnum.NAM);
        Assert.Contains(games, g => g.GameEnum == GameEnum.WW2GI);
        Assert.Contains(games, g => g.GameEnum == GameEnum.Witchaven);
        Assert.Contains(games, g => g.GameEnum == GameEnum.TekWar);
        Assert.Contains(games, g => g.GameEnum == GameEnum.Standalone);
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
    public void IsWangInstalled_WhenNoPath_ReturnsFalse()
    {
        Assert.False((bool)_provider.IsWangInstalled);
    }

    [Fact]
    public void IsFuryInstalled_WhenNoPath_ReturnsFalse()
    {
        Assert.False((bool)_provider.IsFuryInstalled);
    }

    [Fact]
    public void IsRedneckInstalled_WhenNoPath_ReturnsFalse()
    {
        Assert.False((bool)_provider.IsRedneckInstalled);
    }

    [Fact]
    public void IsSlaveInstalled_WhenNoPath_ReturnsFalse()
    {
        Assert.False((bool)_provider.IsSlaveInstalled);
    }

    [Fact]
    public void IsNamInstalled_WhenNoPath_ReturnsFalse()
    {
        Assert.False((bool)_provider.IsNamInstalled);
    }

    [Fact]
    public void IsWW2GIInstalled_WhenNoPath_ReturnsFalse()
    {
        Assert.False((bool)_provider.IsWW2GIInstalled);
    }

    [Fact]
    public void IsWitchavenInstalled_WhenNoPath_ReturnsFalse()
    {
        Assert.False((bool)_provider.IsWitchavenInstalled);
    }

    [Fact]
    public void IsTekWarInstalled_WhenNoPath_ReturnsFalse()
    {
        Assert.False((bool)_provider.IsTekWarInstalled);
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

    [Fact]
    public void GameChangedEvent_FiresForBloodPath()
    {
        GameEnum? received = null;
        _provider.GameChangedEvent += g => received = g;

        _configMock.Raise(x => x.ParameterChangedEvent += null, "PathBlood");

        Assert.Equal(GameEnum.Blood, received);
    }

    [Fact]
    public void GameChangedEvent_FiresForWangPath()
    {
        GameEnum? received = null;
        _provider.GameChangedEvent += g => received = g;

        _configMock.Raise(x => x.ParameterChangedEvent += null, "PathWang");

        Assert.Equal(GameEnum.Wang, received);
    }

    [Fact]
    public void GameChangedEvent_FiresForFuryPath()
    {
        GameEnum? received = null;
        _provider.GameChangedEvent += g => received = g;

        _configMock.Raise(x => x.ParameterChangedEvent += null, "PathFury");

        Assert.Equal(GameEnum.Fury, received);
    }

    [Fact]
    public void GameChangedEvent_FiresForSlavePath()
    {
        GameEnum? received = null;
        _provider.GameChangedEvent += g => received = g;

        _configMock.Raise(x => x.ParameterChangedEvent += null, "PathSlave");

        Assert.Equal(GameEnum.Slave, received);
    }

    [Fact]
    public void GameChangedEvent_FiresForNamPath()
    {
        GameEnum? received = null;
        _provider.GameChangedEvent += g => received = g;

        _configMock.Raise(x => x.ParameterChangedEvent += null, "PathNam");

        Assert.Equal(GameEnum.NAM, received);
    }

    [Fact]
    public void GameChangedEvent_FiresForWW2GIPath()
    {
        GameEnum? received = null;
        _provider.GameChangedEvent += g => received = g;

        _configMock.Raise(x => x.ParameterChangedEvent += null, "PathWW2GI");

        Assert.Equal(GameEnum.WW2GI, received);
    }

    [Fact]
    public void GameChangedEvent_FiresForTekWarPath()
    {
        GameEnum? received = null;
        _provider.GameChangedEvent += g => received = g;

        _configMock.Raise(x => x.ParameterChangedEvent += null, "PathTekWar");

        Assert.Equal(GameEnum.TekWar, received);
    }

    [Fact]
    public void GameChangedEvent_DoesNotFireForUnknownPath()
    {
        var fired = false;
        _provider.GameChangedEvent += _ => fired = true;

        _configMock.Raise(x => x.ParameterChangedEvent += null, "UnknownPath");

        Assert.False(fired);
    }

    [Fact]
    public void GameChangedEvent_DoesNotFireForNullParameter()
    {
        var fired = false;
        _provider.GameChangedEvent += _ => fired = true;

        _configMock.Raise(x => x.ParameterChangedEvent += null, (string?)null);

        Assert.False(fired);
    }
}
