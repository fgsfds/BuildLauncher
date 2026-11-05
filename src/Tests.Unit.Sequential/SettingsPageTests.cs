using Avalonia;
using Avalonia.Controls;
using Avalonia.Desktop.Pages;
using Avalonia.Desktop.ViewModels;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Core.All.Enums;
using Core.Client.Config;
using Core.Client.Enums;
using Games.Providers;
using Tests.Unit.Sequential.Helpers;

namespace Tests.Unit.Sequential;

/// <summary> Exercises settings controls through their production bindings and commands. </summary>
public sealed class SettingsPageTests
{
    /// <summary> Credential editing is disabled where encrypted storage is unavailable. </summary>
    [AvaloniaFact]
    public void Credentials_EnabledOnlyOnSupportedPlatform()
    {
        HeadlessView.Prepare();
        var config = new ConfigProviderFake { GitHubToken = "fixture token" };
        using var host = new HeadlessView(new SettingsPage { DataContext = new SettingsViewModel(config, new GamesPathsProvider(config)) });
        var box = host.Window.GetVisualDescendants().OfType<TextBox>().Single(x => x.Text == "fixture token");
        Assert.Equal(OperatingSystem.IsWindows(), box.IsEnabled);
        var notice = host.Window.GetVisualDescendants().OfType<TextBlock>()
            .Single(x => x.Text?.StartsWith("Encrypted credential storage", StringComparison.Ordinal) == true);
        Assert.Equal(!OperatingSystem.IsWindows(), notice.IsVisible);
    }

    /// <summary> Theme radio buttons update both saved settings and the application's requested theme. </summary>
    /// <param name="theme">The theme selected by the user.</param>
    [AvaloniaTheory]
    [InlineData(ThemeEnum.Light)]
    [InlineData(ThemeEnum.Dark)]
    [InlineData(ThemeEnum.System)]
    public void Theme_Click_PersistsAndApplies(ThemeEnum theme)
    {
        HeadlessView.Prepare();
        var previous = Application.Current!.RequestedThemeVariant;
        var config = new ConfigProviderFake { Theme = theme == ThemeEnum.Dark ? ThemeEnum.Light : ThemeEnum.Dark };
        try
        {
            using var host = new HeadlessView(new SettingsPage
            {
                DataContext = new SettingsViewModel(config, new GamesPathsProvider(config))
            });
            var button = host.Button(theme.ToString());
            host.Click(button);
            Assert.True(((RadioButton)button).IsChecked);
            Assert.Equal(theme, config.Theme);
            Assert.Equal(theme == ThemeEnum.System ? ThemeVariant.Default : theme == ThemeEnum.Dark ? ThemeVariant.Dark : ThemeVariant.Light,
                Application.Current.RequestedThemeVariant);
        }
        finally
        {
            Application.Current.RequestedThemeVariant = previous;
        }
    }

    /// <summary> Launch options round-trip through checkboxes, including turning them back off. </summary>
    /// <param name="label">The checkbox label.</param>
    [AvaloniaTheory]
    [InlineData("Skip intro")]
    [InlineData("Skip startup window")]
    public void LaunchOption_Click_PersistsBothStates(string label)
    {
        HeadlessView.Prepare();
        var config = new ConfigProviderFake { SkipIntro = false, SkipStartup = false };
        using var host = new HeadlessView(new SettingsPage { DataContext = new SettingsViewModel(config, new GamesPathsProvider(config)) });
        var checkbox = host.Button(label);
        host.Click(checkbox);
        Assert.True(label == "Skip intro" ? config.SkipIntro : config.SkipStartup);
        host.Click(checkbox);
        Assert.False(label == "Skip intro" ? config.SkipIntro : config.SkipStartup);
    }

    /// <summary> Editing a game path updates the correct configuration entry through the text binding. </summary>
    [AvaloniaFact]
    public void GamePath_Edit_Persists()
    {
        HeadlessView.Prepare();
        var config = new ConfigProviderFake();
        config.SetGamePath(GameEnum.Duke3D, "original-game-folder");
        using var host = new HeadlessView(new SettingsPage { DataContext = new SettingsViewModel(config, new GamesPathsProvider(config)) });
        var box = host.Window.GetVisualDescendants().OfType<TextBox>().Single(x => x.Text == "original-game-folder");
        box.Text = "new-game-folder";
        Assert.Equal("new-game-folder", config.GetGamePath(GameEnum.Duke3D));
    }
}
