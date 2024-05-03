using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using BuildLauncher.DI;
using BuildLauncher.ViewModels;
using BuildLauncher.Views;
using Common.Config;
using Common.DI;
using Common.Enums;
using Common.Helpers;
using Common.Tools;
using Games.Providers;
using Microsoft.Extensions.DependencyInjection;
using Ports.Providers;

namespace BuildLauncher;

public sealed partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        LoadBindings();
        SetTheme();

        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = BindingsManager.Provider.GetRequiredService<MainViewModel>();
            var gamesProvider = BindingsManager.Provider.GetRequiredService<GamesProvider>();
            var vmFactory = BindingsManager.Provider.GetRequiredService<ViewModelsFactory>();
            var portsProvider = BindingsManager.Provider.GetRequiredService<PortsProvider>();

            desktop.MainWindow = new MainWindow(vm, gamesProvider, vmFactory, portsProvider);

            desktop.Exit += OnAppExit;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime)
        {
            ThrowHelper.NotImplementedException();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnAppExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        var httpClient = BindingsManager.Provider.GetRequiredService<HttpClientInstance>();
        httpClient.Dispose();
    }

    /// <summary>
    /// Load DI bindings
    /// </summary>
    private static void LoadBindings()
    {
        var container = BindingsManager.Instance;

        CommonBindings.Load(container);
        ViewModelsBindings.Load(container);
        Games.DI.ProvidersBindings.Load(container);
        Ports.DI.ProvidersBindings.Load(container);
        Mods.DI.ProvidersBindings.Load(container);
        Updater.DI.ProvidersBindings.Load(container);
        Tools.DI.ProvidersBindings.Load(container);
    }

    /// <summary>
    /// Set theme from the config
    /// </summary>
    private void SetTheme()
    {
        var theme = BindingsManager.Provider.GetRequiredService<ConfigProvider>().Config.Theme;

        var themeEnum = theme switch
        {
            ThemeEnum.System => ThemeVariant.Default,
            ThemeEnum.Light => ThemeVariant.Light,
            ThemeEnum.Dark => ThemeVariant.Dark,
            _ => ThrowHelper.ArgumentOutOfRangeException<ThemeVariant>(theme.ToString())
        };

        RequestedThemeVariant = themeEnum;
    }
}
