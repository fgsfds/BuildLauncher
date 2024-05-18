using Common.API;
using Common.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Common.DI
{
    public static class CommonBindings
    {
        public static void Load(ServiceCollection container)
        {
            container.AddSingleton<HttpClient>();
            container.AddSingleton<ApiInterface>();

            container.AddTransient<ArchiveTools>();
        }
    }
}
