using System.Net;
using System.Net.Http;
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
using Downloader;
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
        _ = container.AddSingleton<DownloadService>(CreateDownloadService);

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
                });

            _ = container.AddHttpClient(string.Empty, client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
                client.Timeout = TimeSpan.FromSeconds(30);
            });
        }

        _ = container.AddSingleton<ILogger>(sp =>
        {
            var factory = sp.GetRequiredService<ILoggerFactory>();
            return factory.CreateLogger("Default");
        });
    }

    private static DownloadService CreateDownloadService(IServiceProvider provider)
    {
        var conf = new DownloadConfiguration()
        {
            MaximumMemoryBufferBytes = 1024 * 1024 * 64,
            ParallelDownload = true,
            ChunkCount = 4,
            ParallelCount = 4,
            MaximumBytesPerSecond = 0,
            MaxTryAgainOnFailure = 5,
            Timeout = 10000,
            RangeDownload = false,
            ClearPackageOnCompletionWithFailure = true,
            CheckDiskSizeBeforeDownload = true,
            EnableLiveStreaming = false,
            RequestConfiguration =
            {
                KeepAlive = true,
                UserAgent = "BuildLauncher"
            }
        };

        return new DownloadService(conf);
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
