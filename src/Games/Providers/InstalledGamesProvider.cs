using Core.All.Enums;
using Core.Client.Interfaces;
using Games.Games;

namespace Games.Providers;

/// <summary>
///     Provides access to all registered game instances and their install status.
///     Listens to config changes to keep game paths in sync.
/// </summary>
public class InstalledGamesProvider
{
    /// <summary>
    ///     Raised when a game's install path changes via config update.
    /// </summary>
    public event GameChanged? GameChangedEvent;

    /// <summary>
    ///     Represents the method that handles <see cref="GameChangedEvent" />.
    /// </summary>
    /// <param name="game">The game whose install path changed.</param>
    public delegate void GameChanged(GameEnum game);

    private readonly Dictionary<GameEnum, BaseGame> _games = new();
    private readonly Dictionary<string, Action> _configMappings = new();

    /// <summary>
    ///     Whether Blood is installed.
    /// </summary>
    public bool IsBloodInstalled => GetGame(GameEnum.Blood).IsBaseGameInstalled;

    /// <summary>
    ///     Whether any Duke Nukem 3D variant (base, World Tour, or Duke64) is installed.
    /// </summary>
    public bool IsDukeInstalled
    {
        get
        {
            var duke = (DukeGame)GetGame(GameEnum.Duke3D);

            return duke.IsBaseGameInstalled || duke.IsWorldTourInstalled || duke.IsDuke64Installed;
        }
    }

    /// <summary>
    ///     Whether Shadow Warrior is installed.
    /// </summary>
    public bool IsWangInstalled => GetGame(GameEnum.Wang).IsBaseGameInstalled;

    /// <summary>
    ///     Whether Ion Fury is installed.
    /// </summary>
    public bool IsFuryInstalled => GetGame(GameEnum.Fury).IsBaseGameInstalled;

    /// <summary>
    ///     Whether any Redneck Rampage variant (base or Rides Again) is installed.
    /// </summary>
    public bool IsRedneckInstalled
    {
        get
        {
            var redneck = (RedneckGame)GetGame(GameEnum.Redneck);

            return redneck.IsBaseGameInstalled || redneck.IsAgainInstalled;
        }
    }

    /// <summary>
    ///     Whether Powerslave is installed.
    /// </summary>
    public bool IsSlaveInstalled => GetGame(GameEnum.Slave).IsBaseGameInstalled;

    /// <summary>
    ///     whether NAM is installed.
    /// </summary>
    public bool IsNamInstalled => GetGame(GameEnum.NAM).IsBaseGameInstalled;

    /// <summary>
    ///     Whether World War II GI is installed.
    /// </summary>
    public bool IsWW2GIInstalled => GetGame(GameEnum.WW2GI).IsBaseGameInstalled;

    /// <summary>
    ///     Whether any Witchaven variant (base or 2) is installed.
    /// </summary>
    public bool IsWitchavenInstalled
    {
        get
        {
            var witch = (WitchavenGame)GetGame(GameEnum.Witchaven);

            return witch.IsBaseGameInstalled || witch.IsWitchaven2Installed;
        }
    }

    /// <summary>
    ///     Whether TekWar is installed.
    /// </summary>
    public bool IsTekWarInstalled => GetGame(GameEnum.TekWar).IsBaseGameInstalled;

    /// <summary>
    ///     Initializes a new instance of <see cref="InstalledGamesProvider" />.
    /// </summary>
    /// <param name="config">Configuration provider used to read initial game paths.</param>
    public InstalledGamesProvider(IConfigProvider config)
    {
        Register(
            new BloodGame
            {
                GameInstallFolder = config.PathBlood
            },
            (nameof(IConfigProvider.PathBlood), (g, v) => g.GameInstallFolder = v, () => config.PathBlood)
            );

        Register(
            new DukeGame
            {
                GameInstallFolder = config.PathDuke3D,
                Duke64RomPath = config.PathDuke64,
                DukeZHRomPath = config.PathDukeZH,
                DukeWTInstallPath = config.PathDukeWT
            },
            (nameof(IConfigProvider.PathDuke3D), (g, v) => g.GameInstallFolder = v, () => config.PathDuke3D),
            (nameof(IConfigProvider.PathDuke64), (g, v) => ((DukeGame)g).Duke64RomPath = v, () => config.PathDuke64),
            (nameof(IConfigProvider.PathDukeZH), (g, v) => ((DukeGame)g).DukeZHRomPath = v, () => config.PathDukeZH),
            (nameof(IConfigProvider.PathDukeWT), (g, v) => ((DukeGame)g).DukeWTInstallPath = v, () => config.PathDukeWT)
            );

        Register(
            new WangGame
            {
                GameInstallFolder = config.PathWang
            },
            (nameof(IConfigProvider.PathWang), (g, v) => g.GameInstallFolder = v, () => config.PathWang)
            );

        Register(
            new FuryGame
            {
                GameInstallFolder = config.PathFury
            },
            (nameof(IConfigProvider.PathFury), (g, v) => g.GameInstallFolder = v, () => config.PathFury)
            );

        Register(
            new RedneckGame
            {
                GameInstallFolder = config.PathRedneck,
                AgainInstallPath = config.PathRidesAgain
            },
            (nameof(IConfigProvider.PathRedneck), (g, v) => g.GameInstallFolder = v, () => config.PathRedneck),
            (nameof(IConfigProvider.PathRidesAgain), (g, v) => ((RedneckGame)g).AgainInstallPath = v, () => config.PathRidesAgain)
            );

        _games[GameEnum.RidesAgain] = _games[GameEnum.Redneck];

        Register(
            new SlaveGame
            {
                GameInstallFolder = config.PathSlave
            },
            (nameof(IConfigProvider.PathSlave), (g, v) => g.GameInstallFolder = v, () => config.PathSlave)
            );

        Register(
            new NamGame
            {
                GameInstallFolder = config.PathNam
            },
            (nameof(IConfigProvider.PathNam), (g, v) => g.GameInstallFolder = v, () => config.PathNam)
            );

        Register(
            new WW2GIGame
            {
                GameInstallFolder = config.PathWW2GI
            },
            (nameof(IConfigProvider.PathWW2GI), (g, v) => g.GameInstallFolder = v, () => config.PathWW2GI)
            );

        Register(
            new WitchavenGame
            {
                GameInstallFolder = config.PathWitchaven,
                Witchaven2InstallPath = config.PathWitchaven2
            },
            (nameof(IConfigProvider.PathWitchaven), (g, v) => g.GameInstallFolder = v, () => config.PathWitchaven),
            (nameof(IConfigProvider.PathWitchaven2), (g, v) => ((WitchavenGame)g).Witchaven2InstallPath = v, () => config.PathWitchaven2)
            );

        _games[GameEnum.Witchaven2] = _games[GameEnum.Witchaven];

        Register(
            new TekWarGame
            {
                GameInstallFolder = config.PathTekWar
            },
            (nameof(IConfigProvider.PathTekWar), (g, v) => g.GameInstallFolder = v, () => config.PathTekWar)
            );

        Register(new StandaloneGame());

        config.ParameterChangedEvent += OnParameterChanged;
    }

    /// <summary>
    ///     Gets the game instance for the specified game enum.
    /// </summary>
    /// <param name="gameEnum">The game to look up.</param>
    /// <returns>The corresponding <see cref="BaseGame" /> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the enum value is not registered.</exception>
    public BaseGame GetGame(GameEnum gameEnum)
    {
        if (_games.TryGetValue(gameEnum, out var game))
        {
            return game;
        }

        throw new ArgumentOutOfRangeException(nameof(gameEnum), gameEnum, $"Unsupported game enum: {gameEnum}.");
    }

    /// <summary>
    ///     Returns all registered games.
    /// </summary>
    /// <returns>A read-only list of all <see cref="BaseGame" /> instances.</returns>
    public virtual IReadOnlyList<BaseGame> GetGames()
    {
        return [.. _games.Values];
    }

    private void Register<T>(T game, params (string PropertyName, Action<T, string?> Setter, Func<string?> Getter)[] bindings) where T : BaseGame
    {
        _games[game.GameEnum] = game;

        foreach (var (propName, setter, getter) in bindings)
        {
            _configMappings[propName] = () =>
            {
                setter(game, getter());
                GameChangedEvent?.Invoke(game.GameEnum);
            };
        }
    }

    private void OnParameterChanged(string? parameterName)
    {
        if (parameterName is not null && _configMappings.TryGetValue(parameterName, out var update))
        {
            update();
        }
    }
}
