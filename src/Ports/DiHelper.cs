using Core.All.Enums;
using Core.All.Releases;
using Microsoft.Extensions.DependencyInjection;
using Ports.Installer;
using Ports.Ports;
using Ports.Ports.EDuke32;
using Ports.Providers;
using Ports.Releases;

namespace Ports;

/// <summary>
///     Helper for registering port-related dependencies.
/// </summary>
public static class DiHelper
{
    /// <summary>
    ///     Adds dependencies to work with ports.
    /// </summary>
    public static IServiceCollection WithPorts(this IServiceCollection container)
    {
        _ = container.AddSingleton<PortInstallerFactory>();
        _ = container.AddSingleton<PortsProvider>();
        _ = container.AddSingleton<PortStarter>();
        _ = container.AddSingleton<ReleaseProviderBase<PortEnum>, PortsRepoReleasesProvider>();

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
        _ = container.AddSingleton<BasePort, ZHRecomp>();

        return container;
    }
}
