﻿using ClientCommon.Config;
using ClientCommon.Providers;
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

        private readonly ConfigProvider _config;
        private readonly InstalledAddonsProviderFactory _installedModsProviderFactory;
        private readonly DownloadableAddonsProviderFactory _downloadableModsProviderFactory;
        private readonly PlaytimeProvider _playtimeProvider;

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
            InstalledAddonsProviderFactory installedModsProviderFactory,
            DownloadableAddonsProviderFactory downloadableModsProviderFactory,
            PlaytimeProvider playtimeProvider
            )
        {
            _config = config;
            _installedModsProviderFactory = installedModsProviderFactory;
            _downloadableModsProviderFactory = downloadableModsProviderFactory;
            _playtimeProvider = playtimeProvider;

            _blood = new(_installedModsProviderFactory, _downloadableModsProviderFactory, _playtimeProvider)
            {
                GameInstallFolder = _config.PathBlood
            };

            _duke3d = new(_installedModsProviderFactory, _downloadableModsProviderFactory, _playtimeProvider)
            {
                GameInstallFolder = _config.PathDuke3D,
                Duke64RomPath = _config.PathDuke64,
                DukeWTInstallPath = _config.PathDukeWT
            };

            _wang = new(_installedModsProviderFactory, _downloadableModsProviderFactory, _playtimeProvider)
            {
                GameInstallFolder = _config.PathWang
            };

            _fury = new(_installedModsProviderFactory, _downloadableModsProviderFactory, _playtimeProvider)
            {
                GameInstallFolder = _config.PathFury
            };

            _redneck = new(_installedModsProviderFactory, _downloadableModsProviderFactory, _playtimeProvider)
            {
                GameInstallFolder = _config.PathRedneck,
                AgainInstallPath = _config.PathRideaAgain
            };

            _slave = new(_installedModsProviderFactory, _downloadableModsProviderFactory, _playtimeProvider)
            {
                GameInstallFolder = _config.PathSlave
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
                GameEnum.ShadowWarrior => _wang,
                GameEnum.Fury => _fury,
                GameEnum.Exhumed => _slave,
                GameEnum.Redneck => _redneck,
                GameEnum.RidesAgain => _redneck,
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
            else if (parameterName.Equals(nameof(_config.PathRideaAgain)))
            {
                _redneck.AgainInstallPath = _config.PathRideaAgain;
                GameChangedEvent?.Invoke(_redneck.GameEnum);
            }
            else if (parameterName.Equals(nameof(_config.PathSlave)))
            {
                _slave.GameInstallFolder = _config.PathSlave;
                GameChangedEvent?.Invoke(_slave.GameEnum);
            }
        }
    }
}
