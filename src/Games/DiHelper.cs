using Games.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Games;

/// <summary>
///     Registers game-related services with the dependency injection container.
/// </summary>
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
