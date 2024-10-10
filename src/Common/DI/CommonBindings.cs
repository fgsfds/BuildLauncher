using Common.Providers;
using Common.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Common.DI;

public static class CommonBindings
{
    public static void Load(ServiceCollection container)
    {
        _ = container.AddSingleton<HttpClient>(CreateHttpClient);
        _ = container.AddSingleton<RepositoriesProvider>();

        _ = container.AddTransient<ArchiveTools>();
    }

    private static HttpClient CreateHttpClient(IServiceProvider service)
    {
        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
        httpClient.Timeout = TimeSpan.FromSeconds(10);
        return httpClient;
    }
}
