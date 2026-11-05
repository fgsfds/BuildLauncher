using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Desktop.ViewModels;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Core.All.Enums;
using Core.Client.Enums;
using Core.Client.Interfaces;

namespace Avalonia.Desktop.Helpers;

/// <summary> Exercises the real published application on its native UI thread. </summary>
internal static class PublishedSmokeTest
{
    /// <summary> Checks startup, catalog deserialization, rendered pages and settings across launches. </summary>
    /// <param name="window">The normally initialized application window.</param>
    /// <param name="lifetime">The native desktop lifetime.</param>
    /// <param name="config">The real SQLite-backed settings.</param>
    /// <param name="api">The offline API using repository catalog fixtures.</param>
    /// <param name="metadataInitialization">The normal startup metadata operation.</param>
    /// <param name="restart">Whether settings must already have been persisted.</param>
    /// <returns>The asynchronous smoke-test operation.</returns>
    #pragma warning disable CA1849 // Write completion synchronously after Close: the UI dispatcher is shutting down.
    internal static async Task RunAsync(MainWindow window, IClassicDesktopStyleApplicationLifetime lifetime,
        IConfigProvider config, IApiInterface api, Task<bool> metadataInitialization, bool restart)
    {
        try
        {
            // Yield until the Opened event and the first native layout have completed.
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
            var tabs = window.FindControl<TabControl>("MainTabs")!;
            var consent = window.FindControl<Grid>("ConsentWindow")!;
            Require(window.IsVisible && window.Bounds.Width > 0, "Native window did not open.");
            Require(await metadataInitialization.ConfigureAwait(true), "Offline metadata initialization failed.");
            Require((await api.GetDataJsonAsync().ConfigureAwait(true))?.Count > 0, "Offline application data is empty.");
            Require((await api.GetAddonsAsync(GameEnum.Duke3D).ConfigureAwait(true))?.Count > 0, "Offline addon catalog is empty.");

            if (restart)
            {
                Require(config.IsConsented && !consent.IsVisible, "Consent was not restored.");
                Require(config.Theme == ThemeEnum.Dark && Application.Current!.RequestedThemeVariant == ThemeVariant.Dark,
                    "Theme was not restored.");
                Require((tabs.SelectedItem as TabItem)?.Name == "ToolsTab", "Active tab was not restored.");
                Require(WindowPlacement.Parse(config.WindowPlacement) is not null, "Window placement was not saved.");
            }
            else
            {
                Require(!config.IsConsented && consent.IsVisible, "Fresh startup did not display consent.");
                consent.GetVisualDescendants().OfType<Button>().Single()
                    .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Require(config.IsConsented && !consent.IsVisible, "Consent action failed.");
                ((MainWindowViewModel)window.DataContext!).SettingsPageViewModel.SetThemeCommand.Execute("Dark");
                Require(config.Theme == ThemeEnum.Dark, "Settings command did not save the theme.");
            }

            foreach (var name in new[] { "SettingsTab", "PortsTab", "ToolsTab", "StandaloneTab", "AboutTab" })
            {
                var tab = window.FindControl<TabItem>(name)!;
                Require(tab.IsEnabled && tab.IsVisible, $"Page {name} is unavailable.");
                tabs.SelectedItem = tab;
                await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
                window.UpdateLayout();
                Require(tab.Content is Control { DataContext: not null, Bounds.Width: > 0, Bounds.Height: > 0 },
                    $"Page {name} did not bind and lay out.");
                using var bitmap = new RenderTargetBitmap(new PixelSize(1300, 800));
                bitmap.Render(window);
                bitmap.Save(Path.Combine(AppContext.BaseDirectory, $"{(restart ? "restart" : "first")}-{name}.png"), new PngBitmapEncoderOptions());
            }

            tabs.SelectedItem = window.FindControl<TabItem>("ToolsTab");
            window.Close();
            Require(config.ActiveTab == "ToolsTab", "Closing the window did not save the active tab.");
            Require(WindowPlacement.Parse(config.WindowPlacement) is not null, "Closing did not save window placement.");
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, restart ? "restart.passed" : "first.passed"), Core.Client.Helpers.ClientProperties.CurrentVersion.ToString());
        }
        catch (Exception ex)
        {
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "smoke-error.txt"), ex.ToString());
            lifetime.Shutdown(1);
        }
    }

    #pragma warning restore CA1849

    /// <summary> Fails the smoke test when an observable application condition is not met. </summary>
    /// <param name="condition">The required condition.</param>
    /// <param name="message">The diagnostic failure message.</param>
    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
