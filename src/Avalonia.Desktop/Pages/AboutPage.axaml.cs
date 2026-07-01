using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Avalonia.Desktop.Pages;

/// <summary>
///     Displays information about the application.
/// </summary>
public sealed partial class AboutPage : UserControl
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AboutPage" /> class.
    /// </summary>
    public AboutPage()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Opens the Patreon page.
    /// </summary>
    private void PatreonClick(object sender, RoutedEventArgs e)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "https://www.patreon.com/fgsfds",
            UseShellExecute = true
        });
    }

    /// <summary>
    ///     Opens the Discord invite link.
    /// </summary>
    private void DiscordClick(object sender, RoutedEventArgs e)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "https://discord.gg/mWvKyxR4et",
            UseShellExecute = true
        });
    }

    /// <summary>
    ///     Opens the GitHub repository page.
    /// </summary>
    private void GitHubClick(object sender, RoutedEventArgs e)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/fgsfds/BuildLauncher",
            UseShellExecute = true
        });
    }

    /// <summary>
    ///     Opens the GitHub issues page.
    /// </summary>
    private void GitHubIssuesClick(object sender, RoutedEventArgs e)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/fgsfds/BuildLauncher/issues/new",
            UseShellExecute = true
        });
    }

    /// <summary>
    ///     Opens the releases page to show the changelog.
    /// </summary>
    private void ShowChangelogClick(object sender, RoutedEventArgs e)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/fgsfds/BuildLauncher/releases",
            UseShellExecute = true
        });
    }
}
