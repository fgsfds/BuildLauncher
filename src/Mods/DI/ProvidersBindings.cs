using Microsoft.Extensions.DependencyInjection;
using Mods.Providers;

namespace Mods.DI
{
    public static class ProvidersBindings
    {
        public static void Load(ServiceCollection container)
        {
            container.AddSingleton<InstalledModsProvider>();
            container.AddSingleton<DownloadableModsProvider>();
        }
    }
}
