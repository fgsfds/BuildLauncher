using Addons.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Addons.DI;

public static class ProvidersBindings
{
    public static void Load(ServiceCollection container)
    {
        _ = container.AddSingleton<InstalledAddonsProviderFactory>();
        _ = container.AddSingleton<DownloadableAddonsProviderFactory>();
        _ = container.AddSingleton<OriginalCampaignsProvider>();
    }
}
