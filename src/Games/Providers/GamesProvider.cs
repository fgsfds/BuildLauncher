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
        private readonly InstalledModsProvider _modsProvider;

        public BloodGame Blood { get; private set; }
        public DukeGame Duke3D { get; private set; }
        public WangGame Wang { get; private set; }


        public GamesProvider(
            ConfigProvider config,
            InstalledModsProvider modsProvider
            )
        {
            _config = config.Config;
            _modsProvider = modsProvider;

            Blood = new(_modsProvider)
            {
                GameInstallFolder = _config.GamePathBlood
            };

            Duke3D = new(_modsProvider)
            {
                GameInstallFolder = _config.GamePathDuke3D,
                Duke64RomPath = _config.GamePathDuke64,
                DukeWTInstallPath = _config.GamePathDukeWT
            };

            Wang = new(_modsProvider)
            {
                GameInstallFolder = _config.GamePathWang
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
                GameEnum.IonFury => ThrowHelper.NotImplementedException<IGame>(),
                GameEnum.Powerslave => ThrowHelper.NotImplementedException<IGame>(),
                GameEnum.NAM => ThrowHelper.NotImplementedException<IGame>(),
                GameEnum.WWIIGI => ThrowHelper.NotImplementedException<IGame>(),
                GameEnum.RedneckRampage => ThrowHelper.NotImplementedException<IGame>(),
                GameEnum.RidesAgain => ThrowHelper.NotImplementedException<IGame>(),
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
        }
    }
}
