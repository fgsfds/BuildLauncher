using System.Net;
using Common.All.Helpers;
using Common.All.Providers;
using Common.Client.Api;
using Common.Client.Config;
using Common.Client.Helpers;
using Common.Client.Interfaces;
using Common.Client.Providers;
using Common.Client.Tools;
using Database.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.Client.DI;

public static class ClientBindings
{
    public static void Load(ServiceCollection container, bool isDesigner)
    {
        _ = container.AddTransient<ArchiveTools>();

        _ = container.AddSingleton<AppUpdateInstaller>();
        _ = container.AddSingleton<PlaytimeProvider>();
        _ = container.AddSingleton<IApiInterface, GitHubApiInterface>();
        _ = container.AddSingleton<RatingProvider>();
        _ = container.AddSingleton<HttpClient>(CreateHttpClient);
        _ = container.AddSingleton<FilesUploader>();
        _ = container.AddSingleton<RepoAppReleasesRetriever>();

        if (isDesigner)
        {
            _ = container.AddSingleton<IConfigProvider, ConfigProviderFake>();

            _ = container
                .AddLogging(x => x
                    .ClearProviders()
                    .AddDebug()
                    );
        }
        else
        {
            _ = container.AddSingleton<IConfigProvider, ConfigProvider>();
            _ = container.AddDbContextFactory<DatabaseContext>();

            _ = container
                .AddLogging(x => x
                    .ClearProviders()
                    .AddFile(ClientProperties.PathToLogFile)
                    .AddDebug()
                    );
        }

        _ = container.AddSingleton<ILogger>(sp =>
        {
            var factory = sp.GetRequiredService<ILoggerFactory>();
            return factory.CreateLogger("Default");
        });
    }

    private static HttpClient CreateHttpClient(IServiceProvider service)
    {
        if (ClientProperties.IsOfflineMode)
        {
            return new HttpClient(new FakeHttpMessageHandler());
        }

        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        return httpClient;
    }

    public sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Fake response")
            };

            return Task.FromResult(response);
        }
    }
}
