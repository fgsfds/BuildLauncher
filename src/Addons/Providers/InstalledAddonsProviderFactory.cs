using Addons.Helpers;
using Core.All.Enums;
using Core.Client.Cache;
using Core.Client.Enums;
using Core.Client.Interfaces;
using Games.Games;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Addons.Providers;

/// <summary>
///     Creates and caches <see cref="InstalledAddonsProvider" /> instances per game.
/// </summary>
public sealed class InstalledAddonsProviderFactory : IDisposable
{
    private readonly ICacheAdder<Stream> _bitmapsCache;
    private readonly IConfigProvider _config;
    private readonly Dictionary<GameEnum, InstalledAddonsProvider> _list = [];
    private readonly ILoggerFactory _loggerFactory;
    private readonly MetadataProvider _metadataProvider;
    private readonly OriginalCampaignsProvider _originalCampaignsProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstalledAddonsProviderFactory" /> class.
    /// </summary>
    /// <param name="config">
    ///     The configuration provider.
    /// </param>
    /// <param name="bitmapsCache">
    ///     The bitmaps cache.
    /// </param>
    /// <param name="originalCampaignsProvider">
    ///     The original campaigns provider.
    /// </param>
    /// <param name="metadataProvider">
    ///     The metadata provider.
    /// </param>
    /// <param name="loggerFactory">
    ///     The logger factory.
    /// </param>
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

    /// <summary>
    ///     Gets the installed addons provider for the specified game, creating it if necessary.
    /// </summary>
    /// <param name="game">
    ///     The game.
    /// </param>
    /// <returns>
    ///     The installed addons provider.
    /// </returns>
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

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var provider in _list.Values)
        {
            provider.Dispose();
        }

        _list.Clear();
    }
}
