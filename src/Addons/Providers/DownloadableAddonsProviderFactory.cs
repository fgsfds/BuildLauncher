using Core.All.Enums;
using Core.Client.Interfaces;
using Core.Client.Tools;
using Games.Games;
using Microsoft.Extensions.Logging;

namespace Addons.Providers;

public sealed class DownloadableAddonsProviderFactory
{
    private readonly Dictionary<GameEnum, DownloadableAddonsProvider> _list = [];

    private readonly ArchiveTools _archiveTools;
    private readonly FilesDownloader _filesDownloader;
    private readonly LocalFilesProvider _filesProvider;
    private readonly IApiInterface _apiInterface;
    private readonly InstalledAddonsProviderFactory _installedAddonsProviderFactory;
    private readonly ILoggerFactory _loggerFactory;

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
    /// Get or create singleton instance of the provider.
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
