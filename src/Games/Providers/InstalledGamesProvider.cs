using Core.All.Enums;
using Core.Client.Interfaces;
using Games.Games;

namespace Games.Providers;

/// <summary>
///     Class that provides singleton instances of game types.
/// </summary>
public class InstalledGamesProvider
{
    /// <summary>
    ///     Represents the method that handles game change events.
    /// </summary>
    /// <param name="game">The game that changed.</param>
    public delegate void GameChanged(GameEnum game);


    /// <summary>
    ///     Blood game instance.
    /// </summary>
    private readonly BloodGame _blood;

    private readonly IConfigProvider _config;

    /// <summary>
    ///     Duke Nukem 3D game instance.
    /// </summary>
    private readonly DukeGame _duke3d;

    /// <summary>
    ///     Ion Fury game instance.
    /// </summary>
    private readonly FuryGame _fury;

    /// <summary>
    ///     NAM game instance.
    /// </summary>
    private readonly NamGame _nam;

    /// <summary>
    ///     Redneck Rampage game instance.
    /// </summary>
    private readonly RedneckGame _redneck;

    /// <summary>
    ///     Powerslave game instance.
    /// </summary>
    private readonly SlaveGame _slave;

    /// <summary>
    ///     Standalone game instance.
    /// </summary>
    private readonly StandaloneGame _standalone;

    /// <summary>
    ///     TekWar game instance.
    /// </summary>
    private readonly TekWarGame _tekwar;

    /// <summary>
    ///     Shadow Warrior game instance.
    /// </summary>
    private readonly WangGame _wang;

    /// <summary>
    ///     Witchaven game instance.
    /// </summary>
    private readonly WitchavenGame _witch;

    /// <summary>
    ///     World War II GI game instance.
    /// </summary>
    private readonly WW2GIGame _ww2gi;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstalledGamesProvider" /> class.
    /// </summary>
    public InstalledGamesProvider() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstalledGamesProvider" /> class.
    /// </summary>
    /// <param name="config">Configuration provider.</param>
    public InstalledGamesProvider(IConfigProvider config)
    {
        _config = config;

        _blood = new()
        {
            GameInstallFolder = _config.PathBlood
        };

        _duke3d = new()
        {
            GameInstallFolder = _config.PathDuke3D,
            Duke64RomPath = _config.PathDuke64,
            DukeZHRomPath = _config.PathDukeZH,
            DukeWTInstallPath = _config.PathDukeWT
        };

        _wang = new()
        {
            GameInstallFolder = _config.PathWang
        };

        _fury = new()
        {
            GameInstallFolder = _config.PathFury
        };

        _redneck = new()
        {
            GameInstallFolder = _config.PathRedneck,
            AgainInstallPath = _config.PathRidesAgain
        };

        _slave = new()
        {
            GameInstallFolder = _config.PathSlave
        };

        _nam = new()
        {
            GameInstallFolder = _config.PathNam
        };

        _ww2gi = new()
        {
            GameInstallFolder = _config.PathWW2GI
        };

        _witch = new()
        {
            GameInstallFolder = _config.PathWitchaven,
            Witchaven2InstallPath = _config.PathWitchaven2
        };

        _tekwar = new()
        {
            GameInstallFolder = _config.PathTekWar
        };

        _standalone = new();

        _config.ParameterChangedEvent += OnParameterChanged;
    }

    /// <summary>
    ///     Is Blood installed.
    /// </summary>
    public bool IsBloodInstalled => _blood.IsBaseGameInstalled;
    /// <summary>
    ///     Is Duke Nukem 3D installed.
    /// </summary>
    public bool IsDukeInstalled => _duke3d.IsBaseGameInstalled || _duke3d.IsWorldTourInstalled || _duke3d.IsDuke64Installed;
    /// <summary>
    ///     Is Shadow Warrior installed.
    /// </summary>
    public bool IsWangInstalled => _wang.IsBaseGameInstalled;
    /// <summary>
    ///     Is Ion Fury installed.
    /// </summary>
    public bool IsFuryInstalled => _fury.IsBaseGameInstalled;
    /// <summary>
    ///     Is Redneck Rampage installed.
    /// </summary>
    public bool IsRedneckInstalled => _redneck.IsBaseGameInstalled || _redneck.IsAgainInstalled;
    /// <summary>
    ///     Is Powerslave installed.
    /// </summary>
    public bool IsSlaveInstalled => _slave.IsBaseGameInstalled;
    /// <summary>
    ///     Is NAM installed.
    /// </summary>
    public bool IsNamInstalled => _nam.IsBaseGameInstalled;
    /// <summary>
    ///     Is World War II GI installed.
    /// </summary>
    public bool IsWW2GIInstalled => _ww2gi.IsBaseGameInstalled;
    /// <summary>
    ///     Is Witchaven installed.
    /// </summary>
    public bool IsWitchavenInstalled => _witch.IsBaseGameInstalled || _witch.IsWitchaven2Installed;
    /// <summary>
    ///     Is TekWar installed.
    /// </summary>
    public bool IsTekWarInstalled => _tekwar.IsBaseGameInstalled;
    /// <summary>
    ///     Occurs when a game's install path changes.
    /// </summary>
    public event GameChanged? GameChangedEvent;


    /// <summary>
    ///     Get game by enum.
    /// </summary>
    /// <param name="gameEnum">Game enum.</param>
    public BaseGame GetGame(GameEnum gameEnum)
    {
        return gameEnum switch
        {
            GameEnum.Blood => _blood,
            GameEnum.Duke3D => _duke3d,
            GameEnum.Wang => _wang,
            GameEnum.Fury => _fury,
            GameEnum.Slave => _slave,
            GameEnum.Redneck => _redneck,
            GameEnum.RidesAgain => _redneck,
            GameEnum.NAM => _nam,
            GameEnum.WW2GI => _ww2gi,
            GameEnum.Standalone => _standalone,
            GameEnum.TekWar => _tekwar,
            GameEnum.Witchaven => _witch,
            GameEnum.Witchaven2 => _witch,
            _ => throw new ArgumentOutOfRangeException(nameof(gameEnum), gameEnum, $"Unsupported game enum: {gameEnum}.")
        };
    }

    /// <summary>
    ///     Gets a list of all game instances.
    /// </summary>
    public virtual IReadOnlyList<BaseGame> GetGames()
    {
        return
        [
            _blood,
            _duke3d,
            _wang,
            _fury,
            _slave,
            _redneck,
            _nam,
            _ww2gi,
            _standalone,
            _tekwar,
            _witch
        ];
    }


    /// <summary>
    ///     Handles configuration parameter changes to update game install paths.
    /// </summary>
    /// <param name="parameterName">Name of the changed parameter.</param>
    private void OnParameterChanged(string? parameterName)
    {
        if (parameterName is null)
        {
            return;
        }

        if (parameterName.Equals(nameof(_config.PathBlood)))
        {
            _blood.GameInstallFolder = _config.PathBlood;
            GameChangedEvent?.Invoke(_blood.GameEnum);
        }
        else if (parameterName.Equals(nameof(_config.PathDuke3D)))
        {
            _duke3d.GameInstallFolder = _config.PathDuke3D;
            GameChangedEvent?.Invoke(_duke3d.GameEnum);
        }
        else if (parameterName.Equals(nameof(_config.PathDuke64)))
        {
            _duke3d.Duke64RomPath = _config.PathDuke64;
            GameChangedEvent?.Invoke(_duke3d.GameEnum);
        }
        else if (parameterName.Equals(nameof(_config.PathDukeZH)))
        {
            _duke3d.DukeZHRomPath = _config.PathDukeZH;
            GameChangedEvent?.Invoke(_duke3d.GameEnum);
        }
        else if (parameterName.Equals(nameof(_config.PathDukeWT)))
        {
            _duke3d.DukeWTInstallPath = _config.PathDukeWT;
            GameChangedEvent?.Invoke(_duke3d.GameEnum);
        }
        else if (parameterName.Equals(nameof(_config.PathWang)))
        {
            _wang.GameInstallFolder = _config.PathWang;
            GameChangedEvent?.Invoke(_wang.GameEnum);
        }
        else if (parameterName.Equals(nameof(_config.PathFury)))
        {
            _fury.GameInstallFolder = _config.PathFury;
            GameChangedEvent?.Invoke(_fury.GameEnum);
        }
        else if (parameterName.Equals(nameof(_config.PathRedneck)))
        {
            _redneck.GameInstallFolder = _config.PathRedneck;
            GameChangedEvent?.Invoke(_redneck.GameEnum);
        }
        else if (parameterName.Equals(nameof(_config.PathRidesAgain)))
        {
            _redneck.AgainInstallPath = _config.PathRidesAgain;
            GameChangedEvent?.Invoke(_redneck.GameEnum);
        }
        else if (parameterName.Equals(nameof(_config.PathSlave)))
        {
            _slave.GameInstallFolder = _config.PathSlave;
            GameChangedEvent?.Invoke(_slave.GameEnum);
        }
        else if (parameterName.Equals(nameof(_config.PathNam)))
        {
            _nam.GameInstallFolder = _config.PathNam;
            GameChangedEvent?.Invoke(_nam.GameEnum);
        }
        else if (parameterName.Equals(nameof(_config.PathWW2GI)))
        {
            _ww2gi.GameInstallFolder = _config.PathWW2GI;
            GameChangedEvent?.Invoke(_ww2gi.GameEnum);
        }
        else if (parameterName.Equals(nameof(_config.PathWitchaven)))
        {
            _witch.GameInstallFolder = _config.PathWitchaven;
            GameChangedEvent?.Invoke(_witch.GameEnum);
        }
        else if (parameterName.Equals(nameof(_config.PathWitchaven2)))
        {
            _witch.Witchaven2InstallPath = _config.PathWitchaven2;
            GameChangedEvent?.Invoke(_witch.GameEnum);
        }
        else if (parameterName.Equals(nameof(_config.PathTekWar)))
        {
            _tekwar.GameInstallFolder = _config.PathTekWar;
            GameChangedEvent?.Invoke(_tekwar.GameEnum);
        }
    }
}
