using Common.Client.API;
using Common.Client.Config;
using Common.Client.Providers;
using Database.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Client.DI
{
    public static class ClientBindings
    {
        public static void Load(ServiceCollection container, bool isDesigner)
        {
            container.AddSingleton<AppUpdateInstaller>();
            container.AddSingleton<PlaytimeProvider>();
            container.AddSingleton<ApiInterface>();
            container.AddSingleton<RatingProvider>();

            if (isDesigner)
            {
                container.AddSingleton<IConfigProvider, ConfigProviderFake>();
            }
            else
            {
                container.AddSingleton<IConfigProvider, ConfigProvider>();
                container.AddSingleton<DatabaseContextFactory>();
            }
        }
    }
}
