using System.Collections.Immutable;
using Addons.Helpers;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Desktop.Helpers;
using Avalonia.Desktop.Misc;
using Avalonia.Desktop.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Core.All.Enums;
using Core.Client.Enums;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Core.Client.Providers;
using Database.Client;
using Games;
using Games.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ports;
using Ports.Providers;
using S3;
using Tools;

namespace Avalonia.Desktop;

public sealed class App : Application
{
    private static App _app = null!;
    private static ServiceProvider _services;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

#if DEBUG
        this.AttachDeveloperTools();
#endif

        _app = this;
    }

    public static int Run(AppBuilder builder)
    {
        int code;

        using ClassicDesktopStyleApplicationLifetime lifetime = new()
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose,
        };

        _ = builder.SetupWithLifetime(lifetime);

        LoadBindings();

        FixDatabase();

        using var dbContext = _services.GetRequiredService<IDbContextFactory<DatabaseContext>>().CreateDbContext();
        dbContext.Database.Migrate();

        var config = _services.GetRequiredService<IConfigProvider>();
        var viewLocator = _services.GetRequiredService<ViewLocator>();
        var vmFactory = _services.GetRequiredService<ViewModelsFactory>();
        var logger = _services.GetRequiredService<ILoggerFactory>().CreateLogger<App>();
        var installedGamesProvider = _services.GetRequiredService<InstalledGamesProvider>();

        SetTheme(config.Theme);

        var bitmapsCache = _services.GetRequiredService<BitmapsCache>();
        bitmapsCache.InitializeCache();

        _app.DataTemplates.Add(viewLocator);
        _app.Resources.Add(new("CachedHashToBitmapConverter", new CachedHashToBitmapConverter(bitmapsCache)));

        using MainWindow mainWindow = new(installedGamesProvider, config);

        lifetime.MainWindow = mainWindow;
        lifetime.MainWindow.DataContext = vmFactory.GetMainWindowViewModel();

        //initialize
        _ = NotificationsHelper.NotificationManager;

        if (ClientProperties.IsDeveloperMode)
        {
            logger.LogInformation("Starting in developer mode");
        }

        if (ClientProperties.IsOfflineMode)
        {
            logger.LogInformation("Starting in offline mode");
        }

        logger.LogInformation($"BuildLauncher version: {ClientProperties.CurrentVersion}");
        logger.LogInformation($"Operating system: {Environment.OSVersion}");
        logger.LogInformation($"Working folder is {ClientProperties.WorkingFolder}");

        var portsProvider = _services.GetRequiredService<PortsProvider>();
        RenameSaveFolder(portsProvider);

        var metadataProvider = _services.GetRequiredService<MetadataProvider>();
        _ = metadataProvider.InitializeAsync();

        try
        {
            code = lifetime.Start();
        }
        catch (Exception ex)
        {
            logger?.LogCritical(ex, "== Critical error while running app ==");

            try
            {
                lifetime.Shutdown();
            }
            catch (Exception ex2)
            {
                logger?.LogCritical(ex2, "== Critical error while shutting down app ==");
            }

            throw;
        }

        return code;
    }


    [Obsolete("Remove some time later")]
    private static void RenameSaveFolder(PortsProvider portsProvider)
    {
        ImmutableArray<string> paths =
            [
                portsProvider.GetPort(PortEnum.BuildGDX).PortSavedGamesFolderPath,
                portsProvider.GetPort(PortEnum.VoidSW).PortSavedGamesFolderPath,
                portsProvider.GetPort(PortEnum.PCExhumed).PortSavedGamesFolderPath,
                portsProvider.GetPort(PortEnum.Raze).PortSavedGamesFolderPath
            ];

        foreach (var path in paths)
        {
            var swPath = Path.Combine(path, "Wang", "shadowwarrior");
            var wangPath = Path.Combine(path, "Wang", "wang");

            var exPath = Path.Combine(path, "Slave", "exhumed");
            var slavePath = Path.Combine(path, "Slave", "slave");

            if (Directory.Exists(swPath))
            {
                Directory.Move(swPath, wangPath);
            }

            if (Directory.Exists(exPath))
            {
                Directory.Move(exPath, slavePath);
            }
        }
    }

    /// <summary>
    /// Load DI bindings
    /// </summary>
    private static void LoadBindings()
    {
        ServiceCollection services = new();

        _ = services.WithDebugLogging();

        if (Design.IsDesignMode)
        {
            _ = services.WithFakeConfig();
        }
        else
        {
            _ = services.WithConfig();
            _ = services.WithFileLogging();
            _ = services.WithDatabase();
        }

        if (ClientProperties.IsOfflineMode)
        {
            _ = services.WithOfflineApi();
            _ = services.WithFakeHttpClients();
        }
        else
        {
            _ = services.WithGitHubApi();
            _ = services.WithHttpClients();
        }

        _ = services.WithClient();
        _ = services.WithMVVM();
        _ = services.WithBitmapsCache();
        _ = services.WithS3FilesUploader();

        _ = services.WithGames();
        _ = services.WithPorts();
        _ = services.WithTools();
        _ = services.WithAddons();

        _services?.Dispose();
        _services = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    /// <summary>
    /// Set theme from the config
    /// </summary>
    private static void SetTheme(ThemeEnum theme)
    {
        var themeEnum = theme switch
        {
            ThemeEnum.System => ThemeVariant.Default,
            ThemeEnum.Light => ThemeVariant.Light,
            ThemeEnum.Dark => ThemeVariant.Dark,
            _ => throw new ArgumentOutOfRangeException(theme.ToString())
        };

        _app.RequestedThemeVariant = themeEnum;
    }

    /// <summary>
    /// Remove database logs leftovers.
    /// </summary>
    private static void FixDatabase()
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        var files = Directory.GetFiles(ClientProperties.WorkingFolder);

        foreach (var file in files)
        {
            if (file.EndsWith(".db-wal", StringComparison.OrdinalIgnoreCase)
                || file.EndsWith(".db-shm", StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(file);
            }
        }
    }
}
