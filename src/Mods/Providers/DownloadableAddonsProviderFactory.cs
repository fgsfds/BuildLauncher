using ClientCommon.API;
using Common.Enums;
using Common.Interfaces;
using Common.Tools;

namespace Mods.Providers
{
    public class DownloadableAddonsProviderFactory(
        ArchiveTools archiveTools,
        HttpClient httpClient,
        ApiInterface apiInterface
        )
    {
        private readonly Dictionary<GameEnum, DownloadableAddonsProvider> _list = [];
        private readonly ArchiveTools _archiveTools = archiveTools;
        private readonly HttpClient _httpClient = httpClient;
        private readonly ApiInterface _apiInterface = apiInterface;

        /// <summary>
        /// Get or create singleton instance of the provider
        /// </summary>
        /// <param name="game">Game</param>
        public DownloadableAddonsProvider GetSingleton(IGame game)
        {
            if (_list.TryGetValue(game.GameEnum, out var value))
            {
                return value;
            }

#pragma warning disable CS0618 // Type or member is obsolete
            DownloadableAddonsProvider newProvider = new(game, _archiveTools, _httpClient, _apiInterface);
#pragma warning restore CS0618 // Type or member is obsolete
            _list.Add(game.GameEnum, newProvider);

            return newProvider;
        }
    }
}
