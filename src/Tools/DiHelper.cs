using Core.All.Enums;
using Core.All.Releases;
using Microsoft.Extensions.DependencyInjection;
using Tools.Installer;
using Tools.Releases;
using Tools.Tools;

namespace Tools;

public static class DiHelper
{
    /// <summary>
    /// Adds dependencies to work with tools.
    /// </summary>
    public static IServiceCollection WithTools(this IServiceCollection container)
    {
        _ = container.AddSingleton<ToolInstallerFactory>();
        _ = container.AddSingleton<ReleaseProviderBase<ToolEnum>, ToolsRepoReleasesProvider>();

        _ = container.AddSingleton<BaseTool, Mapster32>();
        _ = container.AddSingleton<BaseTool, XMapEdit>();
        _ = container.AddSingleton<BaseTool, DOSBlood>();

        return container;
    }
}
