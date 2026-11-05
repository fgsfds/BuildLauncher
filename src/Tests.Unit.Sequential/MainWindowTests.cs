using Avalonia.Controls;
using Avalonia.Desktop;
using Avalonia.Desktop.Helpers;
using Avalonia.Desktop.Pages;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Core.Client.Config;
using Core.Client.Interfaces;
using Moq;
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

    /// <summary> Restores each game's sub-tab after reopening the window, including games that were inactive on close. </summary>
    /// <param name="subTabName">The selected Duke sub-tab.</param>
    [AvaloniaTheory]
    [InlineData("MapsTab")]
    [InlineData("ModsTab")]
    public void CloseAndReopen_RestoresEachVisitedGameSubTab(string subTabName)
    {
        EnsureIconsRegistered();
        var config = new ConfigProviderFake { ActiveTab = "DukeTab", UseMica = false };
        config.SetGameSubTab("BloodTab", "ModsTab");
        using (var window = new MainWindow(new InstalledGamesProvider(config), config))
        {
            var duke = window.FindControl<TabItem>("DukeTab")!;
            var standalone = window.FindControl<TabItem>("StandaloneTab")!;
            duke.IsEnabled = true;
            var dukePage = Assert.IsType<GamePage>(duke.Content);
            var standalonePage = Assert.IsType<GamePage>(standalone.Content);
            dukePage.FindControl<TabItem>(subTabName)!.IsVisible = true;
            var mainTabs = window.FindControl<TabControl>("MainTabs")!;

            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(duke, mainTabs.SelectedItem);
            Assert.Equal("CampaignsTab", dukePage.SelectedSubTab);
            dukePage.FindControl<TabControl>("GameTabs")!.SelectedItem = dukePage.FindControl<TabItem>(subTabName);
            mainTabs.SelectedItem = standalone;
            Dispatcher.UIThread.RunJobs();
            standalonePage.FindControl<TabControl>("GameTabs")!.SelectedItem = standalonePage.FindControl<TabItem>("DownloadsTab");
            mainTabs.SelectedItem = duke;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(subTabName, dukePage.SelectedSubTab);
            mainTabs.SelectedItem = standalone;
            Dispatcher.UIThread.RunJobs();
            window.Close();
        }

        Assert.Equal(subTabName, config.GetGameSubTab("DukeTab"));
        Assert.Equal("DownloadsTab", config.GetGameSubTab("StandaloneTab"));
        Assert.Equal("ModsTab", config.GetGameSubTab("BloodTab"));

        using var restored = new MainWindow(new InstalledGamesProvider(config), config);
        var restoredDuke = restored.FindControl<TabItem>("DukeTab")!;
        restoredDuke.IsEnabled = true;
        var restoredDukePage = Assert.IsType<GamePage>(restoredDuke.Content);
        restoredDukePage.FindControl<TabItem>(subTabName)!.IsVisible = true;
        var restoredTabs = restored.FindControl<TabControl>("MainTabs")!;
        restored.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.Equal("StandaloneTab", ((TabItem)restoredTabs.SelectedItem!).Name);
        Assert.Equal("DownloadsTab", Assert.IsType<GamePage>(((TabItem)restoredTabs.SelectedItem!).Content).SelectedSubTab);
        restoredTabs.SelectedItem = restoredDuke;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(subTabName, restoredDukePage.SelectedSubTab);
        restored.Close();
    }

    /// <summary> Selects Campaigns when the saved sub-tab is hidden or unknown. </summary>
    /// <param name="savedSubTab">The saved sub-tab name.</param>
    [AvaloniaTheory]
    [InlineData("MapsTab")]
    [InlineData("ModsTab")]
    [InlineData("RemovedTab")]
    public void Open_FallsBackToCampaignsWhenSavedSubTabIsUnavailable(string savedSubTab)
    {
        EnsureIconsRegistered();
        var config = new ConfigProviderFake { ActiveTab = "StandaloneTab", UseMica = false };
        config.SetGameSubTab("StandaloneTab", savedSubTab);
        using var window = new MainWindow(new InstalledGamesProvider(config), config);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var page = Assert.IsType<GamePage>(window.FindControl<TabItem>("StandaloneTab")!.Content);
        Assert.Same(page.FindControl<TabItem>("CampaignsTab"), page.FindControl<TabControl>("GameTabs")!.SelectedItem);
        window.Close();
        Assert.Equal("CampaignsTab", config.GetGameSubTab("StandaloneTab"));
    }

    /// <summary> Resets an unreadable game's sub-tab setting when its page opens. </summary>
    [AvaloniaFact]
    public void Open_ResetsUnreadableGameSubTab()
    {
        EnsureIconsRegistered();
        var config = new Mock<IConfigProvider>();
        config.SetupGet(x => x.ActiveTab).Returns("StandaloneTab");
        config.SetupGet(x => x.IsConsented).Returns(true);
        config.Setup(x => x.GetGameSubTab("StandaloneTab")).Throws(new IOException("Sub-tab unavailable."));
        using var window = new MainWindow(new InstalledGamesProvider(config.Object), config.Object);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var page = Assert.IsType<GamePage>(window.FindControl<TabItem>("StandaloneTab")!.Content);
        Assert.Equal("CampaignsTab", page.SelectedSubTab);
        config.Verify(x => x.SetGameSubTab("StandaloneTab", string.Empty), Times.Once());
        window.Close();
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

    /// <summary>Closing a maximized window saves its current screen and retains its normal size.</summary>
    /// <param name="minimizeBeforeClosing">Whether to minimize the window after it moves.</param>
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Close_MaximizedWindowMoved_PersistsCurrentScreen(bool minimizeBeforeClosing)
    {
        EnsureIconsRegistered();
        var config = new ConfigProviderFake { WindowPlacement = "0;0;1400;900;False", UseMica = false };
        using var window = new MainWindow(new InstalledGamesProvider(config), config);
        window.Show();
        var screen = window.Screens.Primary;
        Assert.NotNull(screen);
        var area = screen.WorkingArea;

        window.Position = new Avalonia.PixelPoint(area.Right + 300, area.Y);
        Dispatcher.UIThread.RunJobs();
        var normalSize = window.Bounds.Size;
        window.WindowState = WindowState.Maximized;
        window.Position = area.TopLeft;
        Dispatcher.UIThread.RunJobs();
        Assert.NotNull(window.Screens.ScreenFromWindow(window));

        if (minimizeBeforeClosing)
        {
            window.WindowState = WindowState.Minimized;
        }

        window.Close();

        var placement = WindowPlacement.Parse(config.WindowPlacement);
        Assert.NotNull(placement);
        Assert.True(placement.IsMaximized);
        Assert.Equal(normalSize, placement.Size);
        Assert.True(area.Contains(placement.Position));
    }

    /// <summary>Attempts to reset a failed read to its default before closing and restores the other setting.</summary>
    /// <param name="setting">The setting whose read fails.</param>
    /// <param name="resetFails">Whether writing the default also fails.</param>
    [AvaloniaTheory]
    [InlineData(nameof(IConfigProvider.WindowPlacement), false)]
    [InlineData(nameof(IConfigProvider.ActiveTab), false)]
    [InlineData(nameof(IConfigProvider.WindowPlacement), true)]
    [InlineData(nameof(IConfigProvider.ActiveTab), true)]
    public void Open_SettingsReadFails_ResetsFailedSettingAndRestoresOther(string setting, bool resetFails)
    {
        EnsureIconsRegistered();
        var placementReadFails = setting == nameof(IConfigProvider.WindowPlacement);
        var config = new Mock<IConfigProvider>();
        if (placementReadFails)
        {
            config.SetupGet(x => x.WindowPlacement).Throws(new IOException("Placement unavailable."));
            config.SetupGet(x => x.ActiveTab).Returns("PortsTab");
            if (resetFails)
            {
                config.SetupSet(x => x.WindowPlacement = string.Empty).Throws(new IOException("Placement reset unavailable."));
            }
        }
        else
        {
            config.SetupGet(x => x.WindowPlacement).Returns("100;60;1400;900;False");
            config.SetupGet(x => x.ActiveTab).Throws(new IOException("Tab unavailable."));
            if (resetFails)
            {
                config.SetupSet(x => x.ActiveTab = string.Empty).Throws(new IOException("Tab reset unavailable."));
            }
        }

        using var window = new MainWindow(new InstalledGamesProvider(config.Object), config.Object);
        window.Show();

        Assert.Equal(placementReadFails ? WindowState.Maximized : WindowState.Normal, window.WindowState);
        Assert.Equal(placementReadFails ? "PortsTab" : "SettingsTab",
            ((TabItem)window.FindControl<TabControl>("MainTabs")!.SelectedItem!).Name);

        if (placementReadFails)
        {
            config.VerifySet(x => x.WindowPlacement = string.Empty, Times.Once());
            config.VerifySet(x => x.ActiveTab = It.IsAny<string>(), Times.Never());
        }
        else
        {
            config.VerifySet(x => x.ActiveTab = string.Empty, Times.Once());
            config.VerifySet(x => x.WindowPlacement = It.IsAny<string>(), Times.Never());
        }

        window.Close();
    }

    /// <summary>A failed write still allows the window to close and the other setting to be saved.</summary>
    /// <param name="setting">The setting whose write fails.</param>
    [AvaloniaTheory]
    [InlineData(nameof(IConfigProvider.WindowPlacement))]
    [InlineData(nameof(IConfigProvider.ActiveTab))]
    public void Close_SettingsWriteFails_StillClosesAndSavesOtherSetting(string setting)
    {
        EnsureIconsRegistered();
        var placementWriteFails = setting == nameof(IConfigProvider.WindowPlacement);
        var config = new Mock<IConfigProvider>();
        config.SetupProperty(x => x.WindowPlacement, "0;0;1300;800;False");
        config.SetupProperty(x => x.ActiveTab, "SettingsTab");
        if (placementWriteFails)
        {
            config.SetupSet(x => x.WindowPlacement = It.IsAny<string>()).Throws(new IOException("Placement unavailable."));
        }
        else
        {
            config.SetupSet(x => x.ActiveTab = It.IsAny<string>()).Throws(new IOException("Tab unavailable."));
        }

        using var window = new MainWindow(new InstalledGamesProvider(config.Object), config.Object);
        var closed = false;
        window.Closed += (_, _) => closed = true;
        window.Show();
        window.Position = new Avalonia.PixelPoint(100, 60);
        window.FindControl<TabControl>("MainTabs")!.SelectedItem = window.FindControl<TabItem>("PortsTab");
        window.Close();

        Assert.True(closed);
        if (placementWriteFails)
        {
            Assert.Equal("PortsTab", config.Object.ActiveTab);
        }
        else
        {
            Assert.Equal(new Avalonia.PixelPoint(100, 60), WindowPlacement.Parse(config.Object.WindowPlacement)?.Position);
        }
    }
}
