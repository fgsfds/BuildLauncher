using System;
using System.Net;
using System.Net.Http.Headers;
using Common.All.Enums;
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
        _ = container.AddTransient<FilesDownloader>();

        _ = container.AddSingleton<AppUpdateInstaller>();
        _ = container.AddSingleton<PlaytimeProvider>();
        _ = container.AddSingleton<RatingProvider>();
        _ = container.AddSingleton<FilesUploader>();
        _ = container.AddSingleton<RepoAppReleasesProvider>();

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

        if (ClientProperties.IsOfflineMode)
        {
            _ = container.AddSingleton<IApiInterface, OfflineApiInterface>();

            _ = container.AddHttpClient<HttpClient>(HttpClientEnum.GitHub.GetDescription(), _ => { })
                .ConfigurePrimaryHttpMessageHandler(() => new FakeHttpMessageHandler());
            _ = container.AddHttpClient<HttpClient>(string.Empty, _ => { })
                .ConfigurePrimaryHttpMessageHandler(() => new FakeHttpMessageHandler());
        }
        else
        {
            _ = container.AddSingleton<IApiInterface, GitHubApiInterface>();

            _ = container.AddHttpClient(HttpClientEnum.GitHub.GetDescription())
                .ConfigureHttpClient((serviceProvider, client) =>
                {
                    var config = serviceProvider.GetRequiredService<IConfigProvider>();
                    client.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
                    client.Timeout = TimeSpan.FromSeconds(30);
                    if (!string.IsNullOrWhiteSpace(config.GitHubToken))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.GitHubToken);
                    }
                })
                .RemoveAllLoggers();

            _ = container.AddHttpClient(HttpClientEnum.Upload.GetDescription())
                .ConfigureHttpClient((serviceProvider, client) =>
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
                    client.Timeout = Timeout.InfiniteTimeSpan;
                })
                .RemoveAllLoggers();

            _ = container.AddHttpClient(string.Empty)
                .ConfigureHttpClient((serviceProvider, client) =>
                {
                    var config = serviceProvider.GetRequiredService<IConfigProvider>();
                    client.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
                    client.Timeout = TimeSpan.FromSeconds(30);
                })
                .RemoveAllLoggers();
        }

        _ = container.AddSingleton<ILogger>(sp =>
        {
            var factory = sp.GetRequiredService<ILoggerFactory>();
            return factory.CreateLogger("Default");
        });
    }

    public sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                Content = new StringContent("Fake response")
            };

            return Task.FromResult(response);
        }
    }
}
