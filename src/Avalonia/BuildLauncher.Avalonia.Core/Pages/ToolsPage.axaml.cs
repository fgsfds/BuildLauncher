using Avalonia.Controls;
using BuildLauncher.ViewModels;
using Common.DI;
using Common.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace BuildLauncher.Pages
{
    public sealed partial class ToolsPage : UserControl
    {
        public ToolsPage()
        {
            InitializeComponent();

            var vmFactory = BindingsManager.Provider.GetRequiredService<ViewModelsFactory>();

            XMAPEDIT.DataContext = vmFactory.GetToolViewModel();
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
