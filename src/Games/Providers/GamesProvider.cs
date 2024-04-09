using Common.Config;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Providers;

namespace Games.Providers
{
    /// <summary>
    /// Class that provides singleton instances of game types
    /// </summary>
    public sealed class GamesProvider
    {
        public delegate void GameChanged(GameEnum game);
        public event GameChanged GameChangedEvent;

        private readonly ConfigEntity _config;
        private readonly InstalledModsProviderFactory _modsProvider;
        private readonly DownloadableModsProviderFactory _downloadableModsProviderFactory;

        private readonly BloodGame _blood;
        private readonly DukeGame _duke3d;
        private readonly WangGame _wang;
        private readonly RedneckGame _redneck;
        private readonly FuryGame _fury;
        private readonly SlaveGame _slave;

        public bool IsBloodInstalled => _blood.IsBaseGameInstalled;
        public bool IsDukeInstalled => _duke3d.IsBaseGameInstalled || _duke3d.IsWorldTourInstalled || _duke3d.IsDuke64Installed;
        public bool IsWangInstalled => _wang.IsBaseGameInstalled;
        public bool IsFuryInstalled => _fury.IsBaseGameInstalled;
        public bool IsRedneckInstalled => _redneck.IsBaseGameInstalled || _redneck.IsAgainInstalled;
        public bool IsSlaveInstalled => _slave.IsBaseGameInstalled;


        public GamesProvider(
            ConfigProvider config,
            InstalledModsProviderFactory modsProvider,
            DownloadableModsProviderFactory downloadableModsProviderFactory
            )
        {
            _config = config.Config;
            _modsProvider = modsProvider;
            _downloadableModsProviderFactory = downloadableModsProviderFactory;

            _blood = new(_modsProvider, _downloadableModsProviderFactory)
            {
                GameInstallFolder = _config.GamePathBlood
            };

            _duke3d = new(_modsProvider, _downloadableModsProviderFactory)
            {
                GameInstallFolder = _config.GamePathDuke3D,
                Duke64RomPath = _config.GamePathDuke64,
                DukeWTInstallPath = _config.GamePathDukeWT
            };

            _wang = new(_modsProvider, _downloadableModsProviderFactory)
            {
                GameInstallFolder = _config.GamePathWang
            };

            _fury = new(_modsProvider, _downloadableModsProviderFactory)
            {
                GameInstallFolder = _config.GamePathFury
            };

            _redneck = new(_modsProvider, _downloadableModsProviderFactory)
            {
                GameInstallFolder = _config.GamePathRedneck,
                AgainInstallPath = _config.GamePathAgain
            };

            _slave = new(_modsProvider, _downloadableModsProviderFactory)
            {
                GameInstallFolder = _config.GamePathSlave
            };

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
                GameEnum.Wang => _wang,
                GameEnum.Fury => _fury,
                GameEnum.Slave => _slave,
                GameEnum.Redneck => _redneck,
                GameEnum.RedneckRA => _redneck,
                GameEnum.NAM => ThrowHelper.NotImplementedException<IGame>(),
                GameEnum.WWIIGI => ThrowHelper.NotImplementedException<IGame>(),
                GameEnum.TekWar => ThrowHelper.NotImplementedException<IGame>(),
                GameEnum.Witchaven => ThrowHelper.NotImplementedException<IGame>(),
                GameEnum.Witchaven2 => ThrowHelper.NotImplementedException<IGame>(),
                _ => ThrowHelper.NotImplementedException<IGame>()
            };
        }


        /// <summary>
        /// Update game instance when path to the game changes in the config
        /// </summary>
        /// <param name="parameterName">Config parameter</param>
        private void OnParameterChanged(string parameterName)
        {
            if (parameterName.Equals(nameof(_config.GamePathBlood)))
            {
                _blood.GameInstallFolder = _config.GamePathBlood;
                GameChangedEvent?.Invoke(_blood.GameEnum);
            }
            else if (parameterName.Equals(nameof(_config.GamePathDuke3D)))
            {
                _duke3d.GameInstallFolder = _config.GamePathDuke3D;
                GameChangedEvent?.Invoke(_duke3d.GameEnum);
            }
            else if (parameterName.Equals(nameof(_config.GamePathDuke64)))
            {
                _duke3d.Duke64RomPath = _config.GamePathDuke64;
                GameChangedEvent?.Invoke(_duke3d.GameEnum);
            }
            else if (parameterName.Equals(nameof(_config.GamePathDukeWT)))
            {
                _duke3d.DukeWTInstallPath = _config.GamePathDukeWT;
                GameChangedEvent?.Invoke(_duke3d.GameEnum);
            }
            else if (parameterName.Equals(nameof(_config.GamePathWang)))
            {
                _wang.GameInstallFolder = _config.GamePathWang;
                GameChangedEvent?.Invoke(_wang.GameEnum);
            }
            else if (parameterName.Equals(nameof(_config.GamePathFury)))
            {
                _fury.GameInstallFolder = _config.GamePathFury;
                GameChangedEvent?.Invoke(_fury.GameEnum);
            }
            else if (parameterName.Equals(nameof(_config.GamePathRedneck)))
            {
                _redneck.GameInstallFolder = _config.GamePathRedneck;
                GameChangedEvent?.Invoke(_redneck.GameEnum);
            }
            else if (parameterName.Equals(nameof(_config.GamePathAgain)))
            {
                _redneck.AgainInstallPath = _config.GamePathAgain;
                GameChangedEvent?.Invoke(_redneck.GameEnum);
            }
            else if (parameterName.Equals(nameof(_config.GamePathSlave)))
            {
                _slave.GameInstallFolder = _config.GamePathSlave;
                GameChangedEvent?.Invoke(_slave.GameEnum);
            }
        }
    }
}
