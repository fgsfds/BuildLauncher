using Common.Config;
using Common.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Common.DI
{
    public static class CommonBindings
    {
        public static void Load(ServiceCollection container)
        {
            container.AddTransient<ArchiveTools>();

            container.AddSingleton<ConfigProvider>();
        }
    }
}
