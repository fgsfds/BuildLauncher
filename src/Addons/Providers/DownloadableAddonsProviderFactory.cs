using Core.Client.Interfaces;
using Core.Client.Tools;
using Games.Games;
using Microsoft.Extensions.Logging;

namespace Addons.Providers;

public sealed class DownloadableAddonsProviderFactory
{
    private readonly IApiInterface _apiInterface;
    private readonly ArchiveTools _archiveTools;
    private readonly FilesDownloader _filesDownloader;
    private readonly InstalledAddonsProviderFactory _installedAddonsProviderFactory;
    private readonly ILoggerFactory _loggerFactory;

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
