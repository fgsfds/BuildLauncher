using Core.All;
using Core.All.Enums;
using Core.Client.Enums;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Games.Games;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Addons.Providers;

/// <summary>
///     Factory for creating and caching <see cref="InstalledAddonsProvider" /> instances per game.
/// </summary>
public sealed class InstalledAddonsProviderFactory
{
    private readonly IChannelSubscriber<DiHelper.LocalFileEvent> _channelPublisher;

    private readonly IConfigProvider _config;

    /// <summary>
    ///     Cached provider instances keyed by game enum.
    /// </summary>
    private readonly Dictionary<GameEnum, InstalledAddonsProvider> _list = [];

    private readonly LocalFilesProvider _localFilesProvider;

    private readonly ILoggerFactory _loggerFactory;

    private readonly MetadataProvider _metadataProvider;

    private readonly OriginalCampaignsProvider _originalCampaignsProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstalledAddonsProviderFactory" /> class.
    /// </summary>
    /// <param name="config">Configuration provider for addon state.</param>
    /// <param name="originalCampaignsProvider">Provides original campaigns.</param>
    /// <param name="metadataProvider">Provides remote metadata update checks.</param>
    /// <param name="localFilesProvider">Scans and caches parsed addon files on disk.</param>
    /// <param name="channelPublisher">Channel that publishes local file events.</param>
    /// <param name="loggerFactory">Factory for creating loggers.</param>
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
    ///     Get or create singleton instance of the provider.
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
