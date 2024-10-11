using Common.Enums;
using Common.Interfaces;
using CommunityToolkit.Diagnostics;
using Games.Games;

namespace Games.Providers;

/// <summary>
/// Class that provides singleton instances of game types
/// </summary>
public sealed class GamesProvider
{
    public delegate void GameChanged(GameEnum game);
    public event GameChanged GameChangedEvent;

    private readonly IConfigProvider _config;

    private readonly BloodGame _blood;
    private readonly DukeGame _duke3d;
    private readonly WangGame _wang;
    private readonly RedneckGame _redneck;
    private readonly FuryGame _fury;
    private readonly SlaveGame _slave;
    private readonly NamGame _nam;
    private readonly WW2GIGame _ww2gi;
    private readonly WitchavenGame _witch;
    private readonly TekWarGame _tekwar;
    private readonly StandaloneGame _standalone;

    public bool IsBloodInstalled => _blood.IsBaseGameInstalled;
    public bool IsDukeInstalled => _duke3d.IsBaseGameInstalled || _duke3d.IsWorldTourInstalled || _duke3d.IsDuke64Installed;
    public bool IsWangInstalled => _wang.IsBaseGameInstalled;
    public bool IsFuryInstalled => _fury.IsBaseGameInstalled;
    public bool IsRedneckInstalled => _redneck.IsBaseGameInstalled || _redneck.IsAgainInstalled;
    public bool IsSlaveInstalled => _slave.IsBaseGameInstalled;
    public bool IsNamInstalled => _nam.IsBaseGameInstalled;
    public bool IsWW2GIInstalled => _ww2gi.IsBaseGameInstalled;
    public bool IsWitchavenInstalled => _witch.IsBaseGameInstalled || _witch.IsWitchaven2Installed;
    public bool IsTekWarInstalled => _tekwar.IsBaseGameInstalled;


    public GamesProvider(IConfigProvider config)
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
    /// Get game by enum
    /// </summary>
    /// <param name="gameEnum">Game enum</param>
    public IGame GetGame(GameEnum gameEnum)
    {
        return gameEnum switch
        {
            GameEnum.Blood => _blood,
            GameEnum.Duke3D => _duke3d,
            GameEnum.ShadowWarrior => _wang,
            GameEnum.Fury => _fury,
            GameEnum.Exhumed => _slave,
            GameEnum.Redneck => _redneck,
            GameEnum.RidesAgain => _redneck,
            GameEnum.NAM => _nam,
            GameEnum.WW2GI => _ww2gi,
            GameEnum.Standalone => _standalone,
            GameEnum.TekWar => _tekwar,
            GameEnum.Witchaven => _witch,
            GameEnum.Witchaven2 => _witch,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<IGame>()
        };
    }


    /// <summary>
    /// Update game instance when path to the game changes in the config
    /// </summary>
    /// <param name="parameterName">Config parameter</param>
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
        else if (parameterName.Equals(nameof(_config.PathTekWar)))
        {
            _tekwar.GameInstallFolder = _config.PathTekWar;
            GameChangedEvent?.Invoke(_tekwar.GameEnum);
        }
    }
}
