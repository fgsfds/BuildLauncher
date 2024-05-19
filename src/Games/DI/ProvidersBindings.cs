using Games.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Games.DI
{
    public static class ProvidersBindings
    {
        public static void Load(ServiceCollection container)
        {
            container.AddSingleton<GamesProvider>();
            container.AddSingleton<GamesPathsProvider>();
        }
    }
}
