using Avalonia.Controls;
using BuildLauncher.ViewModels;
using Common.DI;
using Microsoft.Extensions.DependencyInjection;

namespace BuildLauncher.Pages
{
    public sealed partial class DevPage : UserControl
    {
        public DevPage()
        {
            var vm = BindingsManager.Provider.GetRequiredService<DevViewModel>();

            DataContext = vm;

            InitializeComponent();
        }
    }
}
