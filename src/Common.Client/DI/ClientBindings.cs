using Common.Client.Api;
using Common.Client.Config;
using Common.Client.Helpers;
using Common.Client.Interfaces;
using Common.Client.Providers;
using Common.Client.Tools;
using Common.Common.Providers;
using Common.Helpers;
using Database.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.Client.DI;

public static class ClientBindings
{
    public static void Load(ServiceCollection container, bool isDesigner)
    {
        _ = container.AddSingleton<AppUpdateInstaller>();
        _ = container.AddSingleton<PlaytimeProvider>();
        _ = container.AddSingleton<IApiInterface, GitHubApiInterface>();
        _ = container.AddSingleton<RatingProvider>();
        _ = container.AddSingleton<HttpClient>(CreateHttpClient);
        _ = container.AddTransient<ArchiveTools>();
        _ = container.AddSingleton<FilesUploader>();

        _ = container.AddSingleton<AppReleasesProvider>();
        _ = container.AddSingleton<PortsReleasesProvider>();
        _ = container.AddSingleton<RepositoriesProvider>();
        _ = container.AddSingleton<ToolsReleasesProvider>();

        if (isDesigner)
        {
            using var factory = LoggerFactory.Create(builder => builder.AddDebug());
            var logger = factory.CreateLogger("Debug logger");

            _ = container.AddSingleton<ILogger>(logger);
            _ = container.AddSingleton<IConfigProvider, ConfigProviderFake>();
        }
        else
        {
            _ = container.AddSingleton<ILogger>(CreateLogger);
            _ = container.AddSingleton<IConfigProvider, ConfigProvider>();
            _ = container.AddSingleton<DatabaseContextFactory>();
        }
    }

    private static ILogger CreateLogger(IServiceProvider service)
    {
        var logFilePath = Path.Combine(ClientProperties.WorkingFolder, "BuildLauncher.log");
        var logger = FileLoggerFactory.Create(logFilePath);

        return logger;
    }

    private static HttpClient CreateHttpClient(IServiceProvider service)
    {
        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
        httpClient.Timeout = TimeSpan.FromSeconds(10);
        return httpClient;
    }
}
