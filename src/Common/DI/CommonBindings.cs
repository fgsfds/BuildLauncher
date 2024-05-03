using Common.Config;
using Common.Providers;
using Common.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Common.DI
{
    public static class CommonBindings
    {
        public static void Load(ServiceCollection container)
        {
            container.AddSingleton<ConfigProvider>();
            container.AddSingleton<PlaytimeProvider>();
            container.AddSingleton<HttpClientInstance>();

            container.AddTransient<ArchiveTools>();
        }
    }
}
