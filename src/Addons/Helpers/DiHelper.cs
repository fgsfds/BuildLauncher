using Addons.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Addons.Helpers;

public static class DiHelper
{
    /// <summary>
    ///     Adds dependencies to work with addons.
    /// </summary>
    public static IServiceCollection WithAddons(this IServiceCollection container)
    {
        _ = container.AddSingleton<InstalledAddonsProviderFactory>();
        _ = container.AddSingleton<DownloadableAddonsProviderFactory>();
        _ = container.AddSingleton<OriginalCampaignsProvider>();
        _ = container.AddSingleton<MetadataProvider>();
        _ = container.AddSingleton<LocalFilesProvider>();

        return container.AddTransient<IAddonDropHelper, AddonDropHelper>();
    }
}
