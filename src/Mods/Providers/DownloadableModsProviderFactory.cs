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

#pragma warning disable CS0618 // Type or member is obsolete
            DownloadableModsProvider newProvider = new(game, _archiveTools);
#pragma warning restore CS0618 // Type or member is obsolete
            _list.Add(game.GameEnum, newProvider);

            return newProvider;
        }
    }
}
