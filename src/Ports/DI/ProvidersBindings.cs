using Microsoft.Extensions.DependencyInjection;
using Ports.Installer;
using Ports.Providers;

namespace Ports.DI
{
    public static class ProvidersBindings
    {
        public static void Load(ServiceCollection container)
        {
            container.AddSingleton<PortsInstallerFactory>();
            container.AddSingleton<PortsProvider>();
            container.AddSingleton<PortsReleasesProvider>();
        }
    }
}
