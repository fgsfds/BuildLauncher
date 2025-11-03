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
        _ = container.AddSingleton<IApiInterface, GitHubApiInterface>();
        _ = container.AddSingleton<RatingProvider>();
        _ = container.AddSingleton<HttpClient>(CreateHttpClient);
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
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Fake response")
            };

            return Task.FromResult(response);
        }
    }
}
