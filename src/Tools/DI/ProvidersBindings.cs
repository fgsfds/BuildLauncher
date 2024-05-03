using Microsoft.Extensions.DependencyInjection;
using Tools.Installer;

namespace Tools.DI
{
    public static class ProvidersBindings
    {
        public static void Load(ServiceCollection container)
        {
            container.AddSingleton<ToolsInstallerFactory>();
            container.AddSingleton<ToolsReleasesProvider>();
        }
    }
}
