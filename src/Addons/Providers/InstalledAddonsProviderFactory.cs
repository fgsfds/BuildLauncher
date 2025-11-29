using Common.All.Enums;
using Common.Client.Cache;
using Common.Client.Interfaces;
using Games.Games;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Addons.Providers;

public sealed class InstalledAddonsProviderFactory
{
    private readonly Dictionary<GameEnum, InstalledAddonsProvider> _list = [];
    private readonly IConfigProvider _config;
    private readonly ICacheAdder<Stream> _bitmapsCache;
    private readonly OriginalCampaignsProvider _originalCampaignsProvider;
    private readonly ILogger _logger;

    public InstalledAddonsProviderFactory(
        IConfigProvider config,
        [FromKeyedServices("Bitmaps")] ICacheAdder<Stream> bitmapsCache,
        OriginalCampaignsProvider originalCampaignsProvider,
        ILogger logger
        )
    {
        _config = config;
        _bitmapsCache = bitmapsCache;
        _originalCampaignsProvider = originalCampaignsProvider;
        _logger = logger;
    }

    /// <summary>
    /// Get or create singleton instance of the provider
    /// </summary>
    /// <param name="game">Game</param>
    public InstalledAddonsProvider GetSingleton(BaseGame game)
    {
        if (_list.TryGetValue(game.GameEnum, out var value))
        {
            return value;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        InstalledAddonsProvider newProvider = new(game, _config, _logger, _bitmapsCache, _originalCampaignsProvider);
#pragma warning restore CS0618 // Type or member is obsolete
        _list.Add(game.GameEnum, newProvider);

        return newProvider;
    }
}
