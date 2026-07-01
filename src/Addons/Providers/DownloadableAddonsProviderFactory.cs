using Core.All.Enums;
using Core.Client.Interfaces;
using Core.Client.Tools;
using Games.Games;
using Microsoft.Extensions.Logging;

namespace Addons.Providers;

/// <summary>
///     Factory for creating and caching <see cref="DownloadableAddonsProvider" /> instances per game.
/// </summary>
public sealed class DownloadableAddonsProviderFactory
{
    private readonly IApiInterface _apiInterface;

    private readonly ArchiveTools _archiveTools;

    private readonly FilesDownloader _filesDownloader;

    private readonly LocalFilesProvider _filesProvider;

    private readonly InstalledAddonsProviderFactory _installedAddonsProviderFactory;

    /// <summary>
    ///     Cached provider instances keyed by game enum.
    /// </summary>
    private readonly Dictionary<GameEnum, DownloadableAddonsProvider> _list = [];

    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DownloadableAddonsProviderFactory" /> class.
    /// </summary>
    /// <param name="archiveTools">Archive tools for extraction.</param>
    /// <param name="filesDownloader">File downloader service.</param>
    /// <param name="filesProvider">Local files provider.</param>
    /// <param name="apiInterface">API interface for fetching addon data.</param>
    /// <param name="installedAddonsProviderFactory">Factory for installed addons providers.</param>
    /// <param name="loggerFactory">Factory for creating loggers.</param>
    public DownloadableAddonsProviderFactory(
        ArchiveTools archiveTools,
        FilesDownloader filesDownloader,
        LocalFilesProvider filesProvider,
        IApiInterface apiInterface,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        ILoggerFactory loggerFactory
        )
    {
        _archiveTools = archiveTools;
        _filesDownloader = filesDownloader;
        _filesProvider = filesProvider;
        _apiInterface = apiInterface;
        _installedAddonsProviderFactory = installedAddonsProviderFactory;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    ///     Get or create singleton instance of the provider.
    /// </summary>
    /// <param name="game">Game.</param>
    public DownloadableAddonsProvider Get(BaseGame game)
    {
        if (_list.TryGetValue(game.GameEnum, out var value))
        {
            return value;
        }

        #pragma warning disable CS0618 // Type or member is obsolete
        DownloadableAddonsProvider newProvider = new(
            game,
            _archiveTools,
            _filesDownloader,
            _filesProvider,
            _apiInterface,
            _installedAddonsProviderFactory,
            _loggerFactory.CreateLogger<DownloadableAddonsProvider>()
            );
        #pragma warning restore CS0618 // Type or member is obsolete

        _list.Add(game.GameEnum, newProvider);

        return newProvider;
    }
}
