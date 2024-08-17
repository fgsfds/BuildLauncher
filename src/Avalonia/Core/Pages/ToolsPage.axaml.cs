using Avalonia.Controls;
using BuildLauncher.ViewModels;
using Common.Client.Helpers;
using Common.DI;
using Common.Enums;
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

            XMAPEDIT.DataContext = vmFactory.GetToolViewModel(ToolEnum.XMapEdit);
            Mapster32.DataContext = vmFactory.GetToolViewModel(ToolEnum.Mapster32);
        }

        /// <summary>
        /// OpenPortsFolder
        /// </summary>
        private void OpenToolsFolderCommand(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = ClientProperties.ToolsFolderPath,
                UseShellExecute = true,
            });
        }
    }
}
