using Common.Config;
using Common.Enums;
using Common.Tools;
using Games.Providers;
using Mods.Providers;

namespace BuildLauncher.ViewModels
{
    public sealed class GameViewModelFactory
    {
        private readonly GamesProvider _gamesProvider;
        private readonly ArchiveTools _archiveTools;
        private readonly DownloadableModsProvider _modsProvider;
        private readonly ConfigEntity _config;

        public GameViewModelFactory(
            GamesProvider gamesProvider,
            ArchiveTools archiveTools,
            DownloadableModsProvider modsProvider,
            ConfigProvider configProvider)
        {
            _gamesProvider = gamesProvider;
            _archiveTools = archiveTools;
            _modsProvider = modsProvider;
            _config = configProvider.Config;
        }

        /// <summary>
        /// Create <see cref="GameViewModel"/>
        /// </summary>
        /// <param name="gameEnum">Game enum</param>
        public GameViewModel Create(GameEnum gameEnum) =>
            new(
                _gamesProvider.GetGame(gameEnum),
                _gamesProvider, 
                _archiveTools, 
                _modsProvider, 
                _config);
    }
}
