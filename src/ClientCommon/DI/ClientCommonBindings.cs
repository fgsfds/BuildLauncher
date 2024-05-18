using ClientCommon.Config;
using ClientCommon.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace ClientCommon.DI
{
    public static class ClientCommonBindings
    {
        public static void Load(ServiceCollection container)
        {
            container.AddSingleton<AppUpdateInstaller>();
            container.AddSingleton<ConfigProvider>();
            container.AddSingleton<PlaytimeProvider>();
        }
    }
}
