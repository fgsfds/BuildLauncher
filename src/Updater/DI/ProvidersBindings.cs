using Microsoft.Extensions.DependencyInjection;

namespace Updater.DI
{
    public static class ProvidersBindings
    {
        public static void Load(ServiceCollection container)
        {
            container.AddSingleton<AppUpdateInstaller>();
            container.AddSingleton<AppReleasesProvider>();
        }
    }
}
