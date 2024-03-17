using Common.Config;
using Common.Enums;
using Common.Interfaces;

namespace Mods.Providers
{
    public class InstalledModsProviderFactory(ConfigProvider configProvider)
    {
        private readonly Dictionary<GameEnum, InstalledModsProvider> _list = [];
        private readonly ConfigEntity _config = configProvider.Config;

        /// <summary>
        /// Get or create singleton instance of the provider
        /// </summary>
        /// <param name="game">Game</param>
        public InstalledModsProvider GetSingleton(IGame game)
        {
            if (_list.TryGetValue(game.GameEnum, out var value))
            {
                return value;
            }

            InstalledModsProvider newProvider = new(game, _config);
            _list.Add(game.GameEnum, newProvider);

            return newProvider;
        }
    }
}
