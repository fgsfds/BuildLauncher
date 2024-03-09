using Avalonia.Controls;
using BuildLauncher.ViewModels;
using Common.DI;
using Microsoft.Extensions.DependencyInjection;

namespace BuildLauncher.Pages
{
    public sealed partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            var vm = BindingsManager.Provider.GetRequiredService<SettingsViewModel>();

            DataContext = vm;

            InitializeComponent();
        }
    }
}
