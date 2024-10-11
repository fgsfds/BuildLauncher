using Api.Common.Interface;
using Common.Client.Config;
using Common.Client.Helpers;
using Common.Client.Providers;
using Common.Helpers;
using Common.Interfaces;
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
        _ = container.AddSingleton<ApiInterface>();
        _ = container.AddSingleton<RatingProvider>();

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
}
