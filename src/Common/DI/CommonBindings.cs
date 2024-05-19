using Common.Providers;
using Common.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Common.DI
{
    public static class CommonBindings
    {
        public static void Load(ServiceCollection container)
        {
            container.AddSingleton<HttpClient>(CreateHttpClient);
            container.AddSingleton<RepositoriesProvider>();

            container.AddTransient<ArchiveTools>();
        }

        private static HttpClient CreateHttpClient(IServiceProvider provider)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            return httpClient;
        }
    }
}
