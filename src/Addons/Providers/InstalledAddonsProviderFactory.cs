using Common.Client.Interfaces;
using Common.Enums;
using Common.Interfaces;

namespace Addons.Providers;

public sealed class InstalledAddonsProviderFactory
{
    private readonly Dictionary<GameEnum, InstalledAddonsProvider> _list = [];

    private readonly IConfigProvider _config;

    public InstalledAddonsProviderFactory(IConfigProvider config)
    {
        _config = config;
    }

    /// <summary>
    /// Get or create singleton instance of the provider
    /// </summary>
    /// <param name="game">Game</param>
    public InstalledAddonsProvider GetSingleton(IGame game)
    {
        if (_list.TryGetValue(game.GameEnum, out var value))
        {
            return value;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        InstalledAddonsProvider newProvider = new(game, _config);
#pragma warning restore CS0618 // Type or member is obsolete
        _list.Add(game.GameEnum, newProvider);

        return newProvider;
    }
}
