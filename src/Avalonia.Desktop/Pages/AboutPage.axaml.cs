using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Avalonia.Desktop.Pages;

public sealed partial class AboutPage : UserControl
{
    public AboutPage()
    {
        InitializeComponent();
    }

    private void PatreonClick(object sender, RoutedEventArgs e)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "https://www.patreon.com/fgsfds",
            UseShellExecute = true
        });
    }

    private void DiscordClick(object sender, RoutedEventArgs e)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "https://discord.gg/mWvKyxR4et",
            UseShellExecute = true
        });
    }

    private void GitHubClick(object sender, RoutedEventArgs e)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/fgsfds/BuildLauncher",
            UseShellExecute = true
        });
    }

    private void GitHubIssuesClick(object sender, RoutedEventArgs e)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/fgsfds/BuildLauncher/issues/new",
            UseShellExecute = true
        });
    }

    private void ShowChangelogClick(object sender, RoutedEventArgs e)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/fgsfds/BuildLauncher/releases",
            UseShellExecute = true
        });
    }
}
