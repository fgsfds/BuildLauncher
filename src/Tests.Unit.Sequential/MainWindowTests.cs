using Avalonia.Controls;
using Avalonia.Desktop;
using Avalonia.Desktop.Helpers;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Core.Client.Config;
using Games.Providers;
using Optris.Icons.Avalonia;
using Optris.Icons.Avalonia.FontAwesome7;

namespace Tests.Unit.Sequential;

/// <summary> Verifies main window settings across launches. </summary>
public sealed class MainWindowTests
{
    /// <summary> Whether the application icon provider has been registered for window creation. </summary>
    private static bool _iconsRegistered;

    /// <summary> Registers the icon provider that normal application startup supplies. </summary>
    private static void EnsureIconsRegistered()
    {
        if (!_iconsRegistered)
        {
            _ = IconProvider.Current.Register<FontAwesome7IconProvider>();
            _iconsRegistered = true;
        }
    }

    /// <summary> Restores available tabs and falls back to Settings for unavailable tabs when no games are installed. </summary>
    /// <param name="savedTab">The persisted tab name.</param>
    /// <param name="expectedTab">The tab expected after startup.</param>
    [AvaloniaTheory]
    [InlineData("PortsTab", "PortsTab")]
    [InlineData("SettingsTab", "SettingsTab")]
    [InlineData("BloodTab", "SettingsTab")]
    [InlineData("RemovedTab", "SettingsTab")]
    [InlineData("", "SettingsTab")]
    public void Open_RestoresAvailableTabOrDefault(string savedTab, string expectedTab)
    {
        EnsureIconsRegistered();
        var config = new ConfigProviderFake { ActiveTab = savedTab, UseMica = false };
        using var window = new MainWindow(new InstalledGamesProvider(config), config);
        window.FindControl<TabItem>("BloodTab")!.IsEnabled = false;

        window.Show();

        Assert.Equal(expectedTab, ((TabItem)window.FindControl<TabControl>("MainTabs")!.SelectedItem!).Name);
        window.Close();
        Assert.Equal(expectedTab, config.ActiveTab);
    }

    /// <summary> Saves normal bounds and the selected tab for a subsequent launch. </summary>
    [AvaloniaFact]
    public void Close_SavesNormalBoundsAndTab()
    {
        EnsureIconsRegistered();
        var config = new ConfigProviderFake { WindowPlacement = "0;0;1300;800;False", UseMica = false };
        using (var window = new MainWindow(new InstalledGamesProvider(config), config))
        {
            window.Show();
            window.Width = 1400;
            window.Height = 900;
            window.Position = new Avalonia.PixelPoint(100, 60);
            Dispatcher.UIThread.RunJobs();
            window.FindControl<TabControl>("MainTabs")!.SelectedItem = window.FindControl<TabItem>("PortsTab");
            window.Close();
        }

        var placement = WindowPlacement.Parse(config.WindowPlacement);
        Assert.NotNull(placement);
        Assert.False(placement.IsMaximized);
        Assert.Equal(new Avalonia.Size(1400, 900), placement.Size);
        Assert.Equal(new Avalonia.PixelPoint(100, 60), placement.Position);
        Assert.Equal("PortsTab", config.ActiveTab);

        using var restored = new MainWindow(new InstalledGamesProvider(config), config);
        restored.Show();
        Assert.Equal(placement.Position, restored.Position);
        Assert.Equal(placement.Size.Width, restored.Width);
        Assert.Equal(placement.Size.Height, restored.Height);
        Assert.Equal("PortsTab", ((TabItem)restored.FindControl<TabControl>("MainTabs")!.SelectedItem!).Name);
        restored.Close();
    }

    /// <summary> Keeps normal bounds while maximized and avoids reopening minimized. </summary>
    /// <param name="minimized">Whether to minimize the maximized window before closing.</param>
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Close_MaximizedOrMinimized_PreservesNormalBounds(bool minimized)
    {
        EnsureIconsRegistered();
        var config = new ConfigProviderFake { WindowPlacement = "0;0;1300;800;False", UseMica = false };
        using var window = new MainWindow(new InstalledGamesProvider(config), config);
        window.Show();
        window.WindowState = WindowState.Maximized;

        if (minimized)
        {
            window.WindowState = WindowState.Minimized;
        }

        window.Close();

        var placement = WindowPlacement.Parse(config.WindowPlacement);
        Assert.NotNull(placement);
        Assert.True(placement.IsMaximized);
        Assert.Equal(new Avalonia.Size(1300, 800), placement.Size);
    }

    /// <summary> Persistence errors must not prevent the window from closing. </summary>
    /// <param name="setting">The setting whose write fails.</param>
    [AvaloniaTheory]
    [InlineData(nameof(Core.Client.Interfaces.IConfigProvider.WindowPlacement))]
    [InlineData(nameof(Core.Client.Interfaces.IConfigProvider.ActiveTab))]
    public void Close_SettingsWriteFails_StillCloses(string setting)
    {
        EnsureIconsRegistered();
        var config = new Moq.Mock<Core.Client.Interfaces.IConfigProvider>();
        config.SetupGet(x => x.WindowPlacement).Returns("0;0;1300;800;False");
        if (setting == nameof(Core.Client.Interfaces.IConfigProvider.WindowPlacement))
        {
            config.SetupSet(x => x.WindowPlacement = Moq.It.IsAny<string>()).Throws(new IOException("Database unavailable"));
        }
        else
        {
            config.SetupSet(x => x.ActiveTab = Moq.It.IsAny<string>()).Throws(new IOException("Database unavailable"));
        }

        using var window = new MainWindow(new InstalledGamesProvider(config.Object), config.Object);
        var closed = false;
        window.Closed += (_, _) => closed = true;
        window.Show();
        window.Close();
        Assert.True(closed);
    }
}
