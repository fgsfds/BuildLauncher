using Games.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Games;

public static class DiHelper
{
    /// <summary>
    ///     Adds dependencies to work with games.
    /// </summary>
    public static IServiceCollection WithGames(this IServiceCollection container)
    {
        _ = container.AddSingleton<InstalledGamesProvider>();
        _ = container.AddSingleton<GamesPathsProvider>();

        return container;
    }
}
