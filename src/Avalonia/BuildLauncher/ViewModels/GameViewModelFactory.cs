using Common.Enums;
using Common.Tools;
using Games.Providers;
using Mods.Providers;

namespace BuildLauncher.ViewModels
{
    public class GameViewModelFactory
    {
        private readonly GamesProvider _gamesProvider;
        private readonly ArchiveTools _archiveTools;
        private readonly DownloadableModsProvider _modsProvider;

        public GameViewModelFactory(
            GamesProvider gamesProvider,
            ArchiveTools archiveTools,
            DownloadableModsProvider modsProvider)
        {
            _gamesProvider = gamesProvider;
            _archiveTools = archiveTools;
            _modsProvider = modsProvider;
        }

        /// <summary>
        /// Create <see cref="GameViewModel"/>
        /// </summary>
        /// <param name="gameEnum">Game enum</param>
        public GameViewModel Create(GameEnum gameEnum) => new(_gamesProvider.GetGame(gameEnum), _gamesProvider, _archiveTools, _modsProvider);
    }
}
