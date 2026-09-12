using Core.Client.Interfaces;
using Core.Client.Tools;
using Games.Games;
using Microsoft.Extensions.Logging;

namespace Addons.Providers;

/// <summary>
///     Creates <see cref="DownloadableAddonsProvider" /> instances for games.
/// </summary>
public sealed class DownloadableAddonsProviderFactory
{
    private readonly IApiInterface _apiInterface;
    private readonly ArchiveTools _archiveTools;
    private readonly FilesDownloader _filesDownloader;
    private readonly InstalledAddonsProviderFactory _installedAddonsProviderFactory;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DownloadableAddonsProviderFactory" /> class.
    /// </summary>
    /// <param name="archiveTools">
    ///     The archive tools.
    /// </param>
    /// <param name="apiInterface">
    ///     The API interface.
    /// </param>
    /// <param name="filesDownloader">
    ///     The files downloader.
    /// </param>
    /// <param name="installedAddonsProviderFactory">
    ///     The installed addons provider factory.
    /// </param>
    /// <param name="loggerFactory">
    ///     The logger factory.
    /// </param>
    public DownloadableAddonsProviderFactory(
        ArchiveTools archiveTools,
        IApiInterface apiInterface,
        FilesDownloader filesDownloader,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        ILoggerFactory loggerFactory
        )
    {
        _archiveTools = archiveTools;
        _apiInterface = apiInterface;
        _filesDownloader = filesDownloader;
        _installedAddonsProviderFactory = installedAddonsProviderFactory;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    ///     Gets a downloadable addons provider for the specified game.
    /// </summary>
    /// <param name="game">
    ///     The game.
    /// </param>
    /// <returns>
    ///     The downloadable addons provider.
    /// </returns>
    public DownloadableAddonsProvider Get(BaseGame game)
    {
        return new(
            game,
            _archiveTools,
            _filesDownloader,
            _apiInterface,
            _installedAddonsProviderFactory,
            _loggerFactory.CreateLogger<DownloadableAddonsProvider>()
            );
    }
}
