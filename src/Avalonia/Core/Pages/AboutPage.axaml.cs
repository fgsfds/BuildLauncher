using Avalonia.Controls;
using Avalonia.Interactivity;
using BuildLauncher.ViewModels;
using Common.DI;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace BuildLauncher.Pages
{
    public sealed partial class AboutPage : UserControl
    {
        public AboutPage()
        {
            var vm = BindingsManager.Provider.GetRequiredService<AboutViewModel>();

            DataContext = vm;

            InitializeComponent();

            vm.InitializeCommand.Execute(null);
        }

        private void DiscordClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/mWvKyxR4et",
                UseShellExecute = true
            });
        }

        private void GitHubClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/fgsfds/BuildLauncher",
                UseShellExecute = true
            });
        }

        private void GitHubIssuesClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/fgsfds/BuildLauncher/issues/new",
                UseShellExecute = true
            });
        }

        private void ShowChangelogClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/fgsfds/BuildLauncher/releases",
                UseShellExecute = true
            });
        }
    }
}
