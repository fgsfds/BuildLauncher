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
        public event GameChanged NotifyGameChanged;

        private readonly ConfigEntity _config;
        private readonly InstalledModsProviderFactory _modsProvider;
        private readonly DownloadableModsProviderFactory _downloadableModsProviderFactory;

        public BloodGame Blood { get; private set; }
        public DukeGame Duke3D { get; private set; }
        public WangGame Wang { get; private set; }
        public RedneckGame Redneck { get; private set; }
        public FuryGame Fury { get; private set; }
        public SlaveGame Slave { get; private set; }


        public GamesProvider(
            ConfigProvider config,
            InstalledModsProviderFactory modsProvider,
             DownloadableModsProviderFactory downloadableModsProviderFactory
            )
        {
            _config = config.Config;
            _modsProvider = modsProvider;
            _downloadableModsProviderFactory = downloadableModsProviderFactory;

            Blood = new(_modsProvider, _downloadableModsProviderFactory)
            {
                GameInstallFolder = _config.GamePathBlood
            };

            Duke3D = new(_modsProvider, _downloadableModsProviderFactory)
            {
                GameInstallFolder = _config.GamePathDuke3D,
                Duke64RomPath = _config.GamePathDuke64,
                DukeWTInstallPath = _config.GamePathDukeWT
            };

            Wang = new(_modsProvider, _downloadableModsProviderFactory)
            {
                GameInstallFolder = _config.GamePathWang
            };

            Fury = new(_modsProvider, _downloadableModsProviderFactory)
            {
                GameInstallFolder = _config.GamePathFury
            };

            Redneck = new(_modsProvider, _downloadableModsProviderFactory)
            {
                GameInstallFolder = _config.GamePathRedneck,
                AgainInstallPath = _config.GamePathAgain
            };

            Slave = new(_modsProvider, _downloadableModsProviderFactory)
            {
                GameInstallFolder = _config.GamePathSlave
            };

            _config.NotifyParameterChanged += NotifyParameterChanged;
        }


        /// <summary>
        /// Get game by enum
        /// </summary>
        /// <param name="gameEnum">Game enum</param>
        public IGame GetGame(GameEnum gameEnum)
        {
            return gameEnum switch
            {
                GameEnum.Blood => Blood,
                GameEnum.Duke3D => Duke3D,
                GameEnum.Wang => Wang,
                GameEnum.Fury => Fury,
                GameEnum.Slave => Slave,
                GameEnum.NAM => ThrowHelper.NotImplementedException<IGame>(),
                GameEnum.WWIIGI => ThrowHelper.NotImplementedException<IGame>(),
                GameEnum.Redneck => Redneck,
                GameEnum.Again => Redneck,
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
        private void NotifyParameterChanged(string parameterName)
        {
            if (parameterName.Equals(nameof(_config.GamePathBlood)))
            {
                Blood.GameInstallFolder = _config.GamePathBlood;
                NotifyGameChanged?.Invoke(Blood.GameEnum);
            }
            else if (parameterName.Equals(nameof(_config.GamePathDuke3D)))
            {
                Duke3D.GameInstallFolder = _config.GamePathDuke3D;
                NotifyGameChanged?.Invoke(Duke3D.GameEnum);
            }
            else if (parameterName.Equals(nameof(_config.GamePathDuke64)))
            {
                Duke3D.Duke64RomPath = _config.GamePathDuke64;
                NotifyGameChanged?.Invoke(Duke3D.GameEnum);
            }
            else if (parameterName.Equals(nameof(_config.GamePathDukeWT)))
            {
                Duke3D.DukeWTInstallPath = _config.GamePathDukeWT;
                NotifyGameChanged?.Invoke(Duke3D.GameEnum);
            }
            else if (parameterName.Equals(nameof(_config.GamePathWang)))
            {
                Wang.GameInstallFolder = _config.GamePathWang;
                NotifyGameChanged?.Invoke(Wang.GameEnum);
            }
            else if (parameterName.Equals(nameof(_config.GamePathFury)))
            {
                Fury.GameInstallFolder = _config.GamePathFury;
                NotifyGameChanged?.Invoke(Fury.GameEnum);
            }
            else if (parameterName.Equals(nameof(_config.GamePathRedneck)))
            {
                Redneck.GameInstallFolder = _config.GamePathRedneck;
                NotifyGameChanged?.Invoke(Redneck.GameEnum);
            }
            else if (parameterName.Equals(nameof(_config.GamePathAgain)))
            {
                Redneck.AgainInstallPath = _config.GamePathAgain;
                NotifyGameChanged?.Invoke(Redneck.GameEnum);
            }
            else if (parameterName.Equals(nameof(_config.GamePathSlave)))
            {
                Slave.GameInstallFolder = _config.GamePathSlave;
                NotifyGameChanged?.Invoke(Slave.GameEnum);
            }
        }
    }
}
