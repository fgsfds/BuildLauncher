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
                GameInstallFolder = config.GetGamePath(GameEnum.Blood)
            },
            (nameof(GameEnum.Blood), (g, v) => g.GameInstallFolder = v, () => config.GetGamePath(GameEnum.Blood))
            );

        Register(
            new DukeGame
            {
                GameInstallFolder = config.GetGamePath(GameEnum.Duke3D),
                Duke64RomPath = config.GetGamePath(GameEnum.Duke64),
                DukeZHRomPath = config.GetGamePath(GameEnum.DukeZeroHour),
                DukeWTInstallPath = config.PathDukeWT
            },
            (nameof(GameEnum.Duke3D), (g, v) => g.GameInstallFolder = v, () => config.GetGamePath(GameEnum.Duke3D)),
            (nameof(GameEnum.Duke64), (g, v) => ((DukeGame)g).Duke64RomPath = v, () => config.GetGamePath(GameEnum.Duke64)),
            (nameof(GameEnum.DukeZeroHour), (g, v) => ((DukeGame)g).DukeZHRomPath = v, () => config.GetGamePath(GameEnum.DukeZeroHour)),
            (nameof(IConfigProvider.PathDukeWT), (g, v) => ((DukeGame)g).DukeWTInstallPath = v, () => config.PathDukeWT)
            );

        Register(
            new WangGame
            {
                GameInstallFolder = config.GetGamePath(GameEnum.Wang)
            },
            (nameof(GameEnum.Wang), (g, v) => g.GameInstallFolder = v, () => config.GetGamePath(GameEnum.Wang))
            );

        Register(
            new FuryGame
            {
                GameInstallFolder = config.GetGamePath(GameEnum.Fury)
            },
            (nameof(GameEnum.Fury), (g, v) => g.GameInstallFolder = v, () => config.GetGamePath(GameEnum.Fury))
            );

        Register(
            new RedneckGame
            {
                GameInstallFolder = config.GetGamePath(GameEnum.Redneck),
                AgainInstallPath = config.GetGamePath(GameEnum.RidesAgain)
            },
            (nameof(GameEnum.Redneck), (g, v) => g.GameInstallFolder = v, () => config.GetGamePath(GameEnum.Redneck)),
            (nameof(GameEnum.RidesAgain), (g, v) => ((RedneckGame)g).AgainInstallPath = v, () => config.GetGamePath(GameEnum.RidesAgain))
            );

        _games[GameEnum.RidesAgain] = _games[GameEnum.Redneck];

        Register(
            new SlaveGame
            {
                GameInstallFolder = config.GetGamePath(GameEnum.Slave)
            },
            (nameof(GameEnum.Slave), (g, v) => g.GameInstallFolder = v, () => config.GetGamePath(GameEnum.Slave))
            );

        Register(
            new NamGame
            {
                GameInstallFolder = config.GetGamePath(GameEnum.NAM)
            },
            (nameof(GameEnum.NAM), (g, v) => g.GameInstallFolder = v, () => config.GetGamePath(GameEnum.NAM))
            );

        Register(
            new WW2GIGame
            {
                GameInstallFolder = config.GetGamePath(GameEnum.WW2GI)
            },
            (nameof(GameEnum.WW2GI), (g, v) => g.GameInstallFolder = v, () => config.GetGamePath(GameEnum.WW2GI))
            );

        Register(
            new WitchavenGame
            {
                GameInstallFolder = config.GetGamePath(GameEnum.Witchaven),
                Witchaven2InstallPath = config.GetGamePath(GameEnum.Witchaven2)
            },
            (nameof(GameEnum.Witchaven), (g, v) => g.GameInstallFolder = v, () => config.GetGamePath(GameEnum.Witchaven)),
            (nameof(GameEnum.Witchaven2), (g, v) => ((WitchavenGame)g).Witchaven2InstallPath = v, () => config.GetGamePath(GameEnum.Witchaven2))
            );

        _games[GameEnum.Witchaven2] = _games[GameEnum.Witchaven];

        Register(
            new TekWarGame
            {
                GameInstallFolder = config.GetGamePath(GameEnum.TekWar)
            },
            (nameof(GameEnum.TekWar), (g, v) => g.GameInstallFolder = v, () => config.GetGamePath(GameEnum.TekWar))
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
