using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Desktop.DI;
using Avalonia.Desktop.Helpers;
using Avalonia.Desktop.Misc;
using Avalonia.Desktop.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Common.Client.DI;
using Common.Client.Enums;
using Common.Client.Helpers;
using Common.Client.Interfaces;
using CommunityToolkit.Diagnostics;
using Database.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ports.Providers;

namespace Avalonia.Desktop;

public sealed class App : Application
{
    private static App _app = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
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

        using var dbContext = BindingsManager.Provider.GetRequiredService<IDbContextFactory<DatabaseContext>>().CreateDbContext();
        dbContext.Database.Migrate();

        var config = BindingsManager.Provider.GetRequiredService<IConfigProvider>();
        var viewLocator = BindingsManager.Provider.GetRequiredService<ViewLocator>();
        var vmFactory = BindingsManager.Provider.GetRequiredService<ViewModelsFactory>();
        var logger = BindingsManager.Provider.GetRequiredService<ILogger>();

        SetTheme(config.Theme);

        var bitmapsCache = BindingsManager.Provider.GetRequiredService<BitmapsCache>();
        bitmapsCache.InitializeCache();

        _app.DataTemplates.Add(viewLocator);
        _app.Resources.Add(new("CachedHashToBitmapConverter", new CachedHashToBitmapConverter(bitmapsCache)));

        using MainWindow mainWindow = new();

        lifetime.MainWindow = mainWindow;
        lifetime.MainWindow.DataContext = vmFactory.GetMainWindowViewModel();

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

        var portsProvider = BindingsManager.Provider.GetRequiredService<InstalledPortsProvider>();
        RenameSaveFolder(portsProvider);

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
    private static void RenameSaveFolder(InstalledPortsProvider portsProvider)
    {
        IEnumerable<string> paths =
            [
                portsProvider.BuildGDX.PortSavedGamesFolderPath,
                portsProvider.VoidSW.PortSavedGamesFolderPath,
                portsProvider.PCExhumed.PortSavedGamesFolderPath,
                portsProvider.Raze.PortSavedGamesFolderPath
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
        var container = BindingsManager.Instance;

        ClientBindings.Load(container, Design.IsDesignMode);
        GuiBindings.Load(container);
        Games.DI.ProvidersBindings.Load(container);
        Ports.DI.ProvidersBindings.Load(container);
        Addons.DI.ProvidersBindings.Load(container);
        Tools.DI.ProvidersBindings.Load(container);
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
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<ThemeVariant>(theme.ToString())
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
