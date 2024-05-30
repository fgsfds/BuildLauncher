using ClientCommon.API;
using ClientCommon.Config;
using ClientCommon.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace ClientCommon.DI
{
    public static class ClientCommonBindings
    {
        public static void Load(ServiceCollection container, bool isDesigner)
        {
            container.AddSingleton<AppUpdateInstaller>();
            container.AddSingleton<PlaytimeProvider>();
            container.AddSingleton<ApiInterface>();
            container.AddSingleton<ScoresProvider>();

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
