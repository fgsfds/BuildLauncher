using Addons.Helpers;
using Core.All.Enums;
using Core.Client.Cache;
using Core.Client.Enums;
using Core.Client.Interfaces;
using Games.Games;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Addons.Providers;

public sealed class InstalledAddonsProviderFactory
{
    private readonly ICacheAdder<Stream> _bitmapsCache;
    private readonly IConfigProvider _config;
    private readonly Dictionary<GameEnum, InstalledAddonsProvider> _list = [];
    private readonly ILoggerFactory _loggerFactory;
    private readonly MetadataProvider _metadataProvider;
    private readonly OriginalCampaignsProvider _originalCampaignsProvider;

    public InstalledAddonsProviderFactory(
        IConfigProvider config,
        [FromKeyedServices(KeyedServicesEnum.Bitmaps)] ICacheAdder<Stream> bitmapsCache,
        OriginalCampaignsProvider originalCampaignsProvider,
        MetadataProvider metadataProvider,
        ILoggerFactory loggerFactory
        )
    {
        _config = config;
        _bitmapsCache = bitmapsCache;
        _originalCampaignsProvider = originalCampaignsProvider;
        _metadataProvider = metadataProvider;
        _loggerFactory = loggerFactory;
    }

    public InstalledAddonsProvider Get(BaseGame game)
    {
        if (_list.TryGetValue(game.GameEnum, out var value))
        {
            return value;
        }

        #pragma warning disable CS0618 // Type or member is obsolete
        InstalledAddonsProvider newProvider = new(
            game,
            _config,
            _bitmapsCache,
            _originalCampaignsProvider,
            _metadataProvider,
            new ArchivedAddonExtractor(_loggerFactory.CreateLogger<ArchivedAddonExtractor>()),
            _loggerFactory.CreateLogger<InstalledAddonsProvider>()
            );
        #pragma warning restore CS0618 // Type or member is obsolete
        _list.Add(game.GameEnum, newProvider);

        return newProvider;
    }
}
