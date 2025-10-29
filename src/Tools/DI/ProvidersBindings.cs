using Common.All.Enums;
using Common.All.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Tools.Installer;
using Tools.Providers;

namespace Tools.DI;

public static class ProvidersBindings
{
    public static void Load(ServiceCollection container)
    {
        _ = container.AddSingleton<ToolsInstallerFactory>();
        _ = container.AddSingleton<InstalledToolsProvider>();
        _ = container.AddSingleton<IReleaseProvider<ToolEnum>, ToolsReleasesProvider>();
    }
}
