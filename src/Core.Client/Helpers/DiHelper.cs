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

/// <summary>
///     Provides extension methods for registering client services with the dependency injection container.
/// </summary>
public static class DiHelper
{
    /// <summary>
    ///     Registers a fake configuration provider for testing purposes.
    /// </summary>
    /// <param name="container">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection WithFakeConfig(this IServiceCollection container)
    {
        return container.AddSingleton<IConfigProvider, ConfigProviderFake>();
    }

    /// <summary>
    ///     Registers the real configuration provider backed by the database.
    /// </summary>
    /// <param name="container">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection WithConfig(this IServiceCollection container)
    {
        return container.AddSingleton<IConfigProvider, ConfigProvider>();
    }

    /// <summary>
    ///     Registers the database context factory.
    /// </summary>
    /// <param name="container">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection WithDatabase(this IServiceCollection container)
    {
        return container.AddDbContextFactory<DatabaseContext>();
    }

    /// <summary>
    ///     Registers debug output logging.
    /// </summary>
    /// <param name="container">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection WithDebugLogging(this IServiceCollection container)
    {
        return container.AddLogging(x => x.AddDebug());
    }

    /// <summary>
    ///     Registers HTTP clients that return fake responses for testing.
    /// </summary>
    /// <param name="container">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection WithFakeHttpClients(this IServiceCollection container)
    {
        _ = container.AddHttpClient(HttpClientEnum.GitHub.GetDescription())
                     .ConfigurePrimaryHttpMessageHandler(() => new FakeHttpMessageHandler());

        _ = container.AddHttpClient(string.Empty)
                     .ConfigurePrimaryHttpMessageHandler(() => new FakeHttpMessageHandler());

        return container;
    }

    /// <summary>
    ///     Registers HTTP clients with authentication and timeout configuration for production use.
    /// </summary>
    /// <param name="container">The service collection.</param>
    /// <returns>The updated service collection.</returns>
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

    /// <summary>
    ///     Registers the offline API interface for local-only operation.
    /// </summary>
    /// <param name="container">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection WithOfflineApi(this IServiceCollection container)
    {
        return container.AddSingleton<IApiInterface, OfflineApiInterface>();
    }

    /// <summary>
    ///     Registers the GitHub-backed API interface for online operation.
    /// </summary>
    /// <param name="container">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection WithGitHubApi(this IServiceCollection container)
    {
        return container.AddSingleton<IApiInterface, GitHubApiInterface>();
    }

    /// <summary>
    ///     Registers file-based logging to the application log path.
    /// </summary>
    /// <param name="container">The service collection.</param>
    /// <returns>The updated service collection.</returns>
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

    /// <summary>
    ///     Registers core client services including archive tools, downloader, and providers.
    /// </summary>
    /// <param name="container">The service collection.</param>
    /// <returns>The updated service collection.</returns>
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

    /// <summary>
    ///     Registers channel-based event broadcasting services.
    /// </summary>
    /// <param name="container">The service collection.</param>
    /// <returns>The updated service collection.</returns>
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


    /// <summary>
    ///     Represents a local file event notification containing parsed addon file information.
    /// </summary>
    public sealed record LocalFileEvent
    {
        /// <summary>
        ///     The collection of parsed addon files associated with the event.
        /// </summary>
        public IReadOnlyCollection<ParsedAddonFile> Files { get; init; }

        /// <summary>
        ///     Whether the files were added (true) or removed (false).
        /// </summary>
        public bool IsAdded { get; init; }
    }


    /// <summary>
    ///     An HTTP message handler that returns a forbidden fake response for testing.
    /// </summary>
    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        /// <inheritdoc />
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
