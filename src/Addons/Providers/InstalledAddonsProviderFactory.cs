using Core.All;
using Core.All.Enums;
using Core.Client.Enums;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Games.Games;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Addons.Providers;

public sealed class InstalledAddonsProviderFactory
{
    private readonly Dictionary<GameEnum, InstalledAddonsProvider> _list = [];
    private readonly IConfigProvider _config;
    private readonly OriginalCampaignsProvider _originalCampaignsProvider;
    private readonly MetadataProvider _metadataProvider;
    private readonly LocalFilesProvider _localFilesProvider;
    private readonly IChannelSubscriber<DiHelper.LocalFileEvent> _channelPublisher;
    private readonly ILoggerFactory _loggerFactory;

    public InstalledAddonsProviderFactory(
        IConfigProvider config,
        OriginalCampaignsProvider originalCampaignsProvider,
        MetadataProvider metadataProvider,
        LocalFilesProvider localFilesProvider,
        [FromKeyedServices(KeyedServicesEnum.LocalFilesChannel)] IChannelSubscriber<DiHelper.LocalFileEvent> channelPublisher,
        ILoggerFactory loggerFactory
        )
    {
        _config = config;
        _originalCampaignsProvider = originalCampaignsProvider;
        _metadataProvider = metadataProvider;
        _localFilesProvider = localFilesProvider;
        _channelPublisher = channelPublisher;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Get or create singleton instance of the provider.
    /// </summary>
    /// <param name="game">Game.</param>
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
            _originalCampaignsProvider,
            _metadataProvider,
            _localFilesProvider,
            _channelPublisher,
            _loggerFactory.CreateLogger<InstalledAddonsProvider>());
#pragma warning restore CS0618 // Type or member is obsolete
        _list.Add(game.GameEnum, newProvider);

        return newProvider;
    }
}
