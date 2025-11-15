using Microsoft.Extensions.DependencyInjection;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Ports.DI;

public static class PortsBindings
{
    public static void Load(ServiceCollection container)
    {
        _ = container.AddSingleton<BasePort, EDuke32>();
        _ = container.AddSingleton<BasePort, NBlood>();
        _ = container.AddSingleton<BasePort, NotBlood>();
        _ = container.AddSingleton<BasePort, Fury>();
        _ = container.AddSingleton<BasePort, PCExhumed>();
        _ = container.AddSingleton<BasePort, RedNukem>();
        _ = container.AddSingleton<BasePort, VoidSW>();
        _ = container.AddSingleton<BasePort, Raze>();
        _ = container.AddSingleton<BasePort, BuildGDX>();
        _ = container.AddSingleton<BasePort, DosBox>();
    }
}
