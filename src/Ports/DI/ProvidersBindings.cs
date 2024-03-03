using Microsoft.Extensions.DependencyInjection;
using Ports.Providers;
using Ports.Tools;

namespace Ports.DI
{
    public static class ProvidersBindings
    {
        public static void Load(ServiceCollection container)
        {
            container.AddSingleton<PortsInstallerFactory>();
            container.AddSingleton<PortsProvider>();
        }
    }
}
