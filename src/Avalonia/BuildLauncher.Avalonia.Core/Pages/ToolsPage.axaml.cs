using Avalonia.Controls;
using BuildLauncher.ViewModels;
using Common.DI;
using Common.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Tools.Tools;

namespace BuildLauncher.Pages
{
    public sealed partial class ToolsPage : UserControl
    {
        public ToolsPage()
        {
            InitializeComponent();

            var vmFactory = BindingsManager.Provider.GetRequiredService<ViewModelsFactory>();

            XMAPEDIT.DataContext = vmFactory.GetToolViewModel(nameof(XMapEdit));
            Mapster32.DataContext = vmFactory.GetToolViewModel(nameof(Tools.Tools.Mapster32));
        }

        /// <summary>
        /// OpenPortsFolder
        /// </summary>
        private void OpenToolsFolderCommand(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = CommonProperties.ToolsFolderPath,
                UseShellExecute = true,
            });
        }
    }
}
