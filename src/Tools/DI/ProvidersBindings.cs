using Microsoft.Extensions.DependencyInjection;
using Tools.Installer;
using Tools.Providers;

namespace Tools.DI
{
    public static class ProvidersBindings
    {
        public static void Load(ServiceCollection container)
        {
            container.AddSingleton<ToolsInstallerFactory>();
            container.AddSingleton<ToolsReleasesProvider>();
            container.AddSingleton<ToolsProvider>();
        }
    }
}
