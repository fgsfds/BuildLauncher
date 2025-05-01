using Microsoft.Extensions.DependencyInjection;
using Tools.Installer;
using Tools.Providers;

namespace Tools.DI;

public static class ProvidersBindings
{
    public static void Load(ServiceCollection container)
    {
        _ = container.AddSingleton<ToolsInstallerFactory>();
        _ = container.AddSingleton<ToolsReleasesProvider>();
        _ = container.AddSingleton<ToolsProvider>();
    }
}
