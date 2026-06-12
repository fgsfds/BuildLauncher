using Core.All.Enums;
using Core.All.Providers;
using Microsoft.Extensions.DependencyInjection;
using Tools.Installer;
using Tools.Providers;
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
        _ = container.AddSingleton<ReleaseProvider<ToolEnum>, ToolsReleasesProvider>();

        _ = container.AddSingleton<BaseTool, Mapster32>();
        _ = container.AddSingleton<BaseTool, XMapEdit>();
        return container.AddSingleton<BaseTool, DOSBlood>();
    }
}
