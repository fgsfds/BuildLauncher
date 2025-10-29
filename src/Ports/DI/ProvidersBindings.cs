using Common.All.Enums;
using Common.All.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Ports.Installer;
using Ports.Ports;
using Ports.Providers;

namespace Ports.DI;

public static class ProvidersBindings
{
    public static void Load(ServiceCollection container)
    {
        _ = container.AddSingleton<PortsInstallerFactory>();
        _ = container.AddSingleton<InstalledPortsProvider>();
        _ = container.AddSingleton<PortStarter>();
        _ = container.AddSingleton<IReleaseProvider<PortEnum>, PortsReleasesProvider>();
    }
}
