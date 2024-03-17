using Common.Enums;
using Common.Interfaces;
using Common.Tools;

namespace Mods.Providers
{
    public class DownloadableModsProviderFactory(ArchiveTools archiveTools)
    {
        private readonly Dictionary<GameEnum, DownloadableModsProvider> _list = [];
        private readonly ArchiveTools _archiveTools = archiveTools;

        /// <summary>
        /// Get or create singleton instance of the provider
        /// </summary>
        /// <param name="game">Game</param>
        public DownloadableModsProvider GetSingleton(IGame game)
        {
            if (_list.TryGetValue(game.GameEnum, out var value))
            {
                return value;
            }

            DownloadableModsProvider newProvider = new(game, _archiveTools);
            _list.Add(game.GameEnum, newProvider);

            return newProvider;
        }
    }
}
