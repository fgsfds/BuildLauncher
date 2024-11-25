using Common.Client.Interfaces;
using Common.Client.Tools;
using Common.Enums;
using Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Addons.Providers;

public sealed class DownloadableAddonsProviderFactory
{
    private readonly Dictionary<GameEnum, DownloadableAddonsProvider> _list = [];

    private readonly ArchiveTools _archiveTools;
    private readonly IApiInterface _apiInterface;
    private readonly InstalledAddonsProviderFactory _installedAddonsProviderFactory;
    private readonly ILogger _logger;

    public DownloadableAddonsProviderFactory(
        ArchiveTools archiveTools,
        IApiInterface apiInterface,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        ILogger logger
        )
    {
        _archiveTools = archiveTools;
        _apiInterface = apiInterface;
        _installedAddonsProviderFactory = installedAddonsProviderFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get or create singleton instance of the provider
    /// </summary>
    /// <param name="game">Game</param>
    public DownloadableAddonsProvider GetSingleton(IGame game)
    {
        if (_list.TryGetValue(game.GameEnum, out var value))
        {
            return value;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        DownloadableAddonsProvider newProvider = new(
            game,
            _archiveTools,
            _apiInterface,
            _installedAddonsProviderFactory,
            _logger
            );
#pragma warning restore CS0618 // Type or member is obsolete

        _list.Add(game.GameEnum, newProvider);

        return newProvider;
    }
}
