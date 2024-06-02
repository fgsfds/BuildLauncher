using ClientCommon.API;
using Common.Enums;
using Common.Interfaces;
using Common.Tools;

namespace Mods.Providers;

public class DownloadableAddonsProviderFactory
{
    private readonly Dictionary<GameEnum, DownloadableAddonsProvider> _list = [];

    private readonly ArchiveTools _archiveTools;
    private readonly ApiInterface _apiInterface;
    private readonly InstalledAddonsProviderFactory _installedAddonsProviderFactorty;

    public DownloadableAddonsProviderFactory(
        ArchiveTools archiveTools,
        ApiInterface apiInterface,
        InstalledAddonsProviderFactory installedAddonsProviderFactorty
    )
    {
        _archiveTools = archiveTools;
        _apiInterface = apiInterface;
        _installedAddonsProviderFactorty = installedAddonsProviderFactorty;
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
            _installedAddonsProviderFactorty
            );
#pragma warning restore CS0618 // Type or member is obsolete
        _list.Add(game.GameEnum, newProvider);

        return newProvider;
    }
}
