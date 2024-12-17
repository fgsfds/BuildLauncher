using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Core.Plugins;
using Avalonia.Desktop.DI;
using Avalonia.Desktop.Helpers;
using Avalonia.Desktop.Views;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Common.Client.DI;
using Common.Client.Enums;
using Common.Client.Helpers;
using Common.Client.Interfaces;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Avalonia.Desktop;

public sealed class App : Application
{
    public static WindowNotificationManager NotificationManager { get; private set; }
    public static Random Random { get; private set; }

    //private static readonly Mutex _mutex = new(false, "BuildLauncher");

    private static ILogger? _logger = null;


    static App()
    {
        NotificationManager = null!;
        Random = null!;
    }


    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        //if (!Design.IsDesignMode)
        //{
        //    if (!DoesHaveWriteAccess(ClientProperties.WorkingFolder))
        //    {
        //        var messageBox = new MessageBox($"""
        //        Superheater doesn't have write access to
        //        {ClientProperties.WorkingFolder}
        //        and can't be launched. 
        //        Move it to the folder where you have write access.
        //        """);
        //        messageBox.Show();
        //        return;
        //    }

        //    if (!_mutex.WaitOne(1000, false))
        //    {
        //        var messageBox = new MessageBox($"You can't launch multiple instances of Superheater");
        //        messageBox.Show();
        //        return;
        //    }
        //}

        RenameConfig();
        LoadBindings();

        _logger = BindingsManager.Provider.GetRequiredService<ILogger>();

        try
        {
            SetTheme();

            if (ClientProperties.IsDeveloperMode)
            {
                _logger.LogInformation("Started in developer mode");
            }

            _logger.LogInformation($"BuildLauncher version: {ClientProperties.CurrentVersion}");
            _logger.LogInformation($"Operating system: {Environment.OSVersion}");
            _logger.LogInformation($"Working folder is {ClientProperties.WorkingFolder}");

            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // DI entry point
                desktop.MainWindow = BindingsManager.Provider.GetRequiredService<MainWindow>();

                desktop.Exit += OnAppExit;
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime)
            {
                ThrowHelper.ThrowNotSupportedException();
            }

            NotificationManager = new(AvaloniaProperties.TopLevel)
            {
                MaxItems = 3,
                Position = NotificationPosition.TopRight,
                Margin = new(0, 50, 10, 0)
            };

            Random = new();

            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception ex)
        {
            if (Design.IsDesignMode)
            {
                return;
            }

            _logger?.LogCritical(ex, "== Critical error ==");

            Environment.FailFast(Environment.StackTrace);
        }
    }

    [Obsolete]
    private static void RenameConfig()
    {
        var oldConfigPath = Path.Combine(ClientProperties.WorkingFolder, "config.db");
        var newConfigPath = Path.Combine(ClientProperties.WorkingFolder, "BuildLauncher.db");

        if (File.Exists(oldConfigPath))
        {
            File.Move(oldConfigPath, newConfigPath, true);
        }
    }

    private void OnAppExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        var httpClient = BindingsManager.Provider.GetRequiredService<HttpClient>();
        httpClient.Dispose();
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
    private void SetTheme()
    {
        var theme = BindingsManager.Provider.GetRequiredService<IConfigProvider>().Theme;

        var themeEnum = theme switch
        {
            ThemeEnum.System => ThemeVariant.Default,
            ThemeEnum.Light => ThemeVariant.Light,
            ThemeEnum.Dark => ThemeVariant.Dark,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<ThemeVariant>(theme.ToString())
        };

        RequestedThemeVariant = themeEnum;
    }
}
