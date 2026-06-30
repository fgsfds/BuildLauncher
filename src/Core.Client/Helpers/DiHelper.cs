using System.Net;
using System.Net.Http.Headers;
using Core.All;
using Core.All.Enums;
using Core.All.Helpers;
using Core.All.Releases;
using Core.Client.Api;
using Core.Client.Config;
using Core.Client.Enums;
using Core.Client.Interfaces;
using Core.Client.Providers;
using Core.Client.Releases;
using Core.Client.Tools;
using Database.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;

namespace Core.Client.Helpers;

public static class DiHelper
{
    public static IServiceCollection WithFakeConfig(this IServiceCollection container)
    {
        return container.AddSingleton<IConfigProvider, ConfigProviderFake>();
    }

    public static IServiceCollection WithConfig(this IServiceCollection container)
    {
        return container.AddSingleton<IConfigProvider, ConfigProvider>();
    }

    public static IServiceCollection WithDatabase(this IServiceCollection container)
    {
        return container.AddDbContextFactory<DatabaseContext>();
    }

    public static IServiceCollection WithDebugLogging(this IServiceCollection container)
    {
        return container.AddLogging(x => x.AddDebug());
    }

    public static IServiceCollection WithFakeHttpClients(this IServiceCollection container)
    {
        _ = container.AddHttpClient(HttpClientEnum.GitHub.GetDescription())
                     .ConfigurePrimaryHttpMessageHandler(() => new FakeHttpMessageHandler());

        _ = container.AddHttpClient(string.Empty)
                     .ConfigurePrimaryHttpMessageHandler(() => new FakeHttpMessageHandler());

        return container;
    }

    public static IServiceCollection WithHttpClients(this IServiceCollection container)
    {
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

        _ = container.AddHttpClient(string.Empty)
                     .ConfigureHttpClient((serviceProvider, client) =>
                      {
                          var config = serviceProvider.GetRequiredService<IConfigProvider>();
                          client.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
                          client.Timeout = TimeSpan.FromSeconds(30);
                      })
                     .RemoveAllLoggers();

        return container;
    }

    public static IServiceCollection WithOfflineApi(this IServiceCollection container)
    {
        return container.AddSingleton<IApiInterface, OfflineApiInterface>();
    }

    public static IServiceCollection WithGitHubApi(this IServiceCollection container)
    {
        return container.AddSingleton<IApiInterface, GitHubApiInterface>();
    }

    public static IServiceCollection WithFileLogging(this IServiceCollection container)
    {
        return container
           .AddLogging(x => x
                           .AddFile(
                                ClientProperties.PathToLogFile,
                                opt =>
                                {
                                    opt.Append = false;
                                    opt.FormatLogFileName = (fileName) => { return string.Format(fileName, DateTime.UtcNow); };
                                    opt.FormatLogEntry = (message) => { return $"[{DateTime.Now.ToLocalTime() + "]",-25} {message.LogLevel,-15} {message.Message} {message.Exception}"; };
                                })
                           .AddFilter("System.Net.Http.HttpClient", LogLevel.None)
                           .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None)
                );
    }

    public static IServiceCollection WithClient(this IServiceCollection container)
    {
        _ = container.AddTransient<ArchiveTools>();
        _ = container.AddTransient<FilesDownloader>();
        _ = container.AddSingleton<AppUpdateInstaller>();
        _ = container.AddSingleton<PlaytimeProvider>();
        _ = container.AddSingleton<RatingProvider>();
        _ = container.AddSingleton<AddonsDatabaseManager>();
        _ = container.AddSingleton<ReleaseProviderBase<AppReleaseEnum>, AppRepoReleasesProvider>();

        return container;
    }

    public static IServiceCollection WithChannels(this IServiceCollection container)
    {
        _ = container.AddSingleton<ChannelBroadcaster<LocalFileEvent>>();

        _ = container.AddKeyedSingleton<IChannelSubscriber<LocalFileEvent>>(
            KeyedServicesEnum.LocalFilesChannel,
            (sp, _) => sp.GetRequiredService<ChannelBroadcaster<LocalFileEvent>>());

        _ = container.AddKeyedSingleton<IChannelPublisher<LocalFileEvent>>(
            KeyedServicesEnum.LocalFilesChannel,
            (sp, _) => sp.GetRequiredService<ChannelBroadcaster<LocalFileEvent>>());

        return container;
    }


    public sealed record LocalFileEvent
    {
        public IReadOnlyCollection<ParsedAddonFile> Files { get; init; }
        public bool IsAdded { get; init; }
    }


    private sealed class FakeHttpMessageHandler : HttpMessageHandler
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
