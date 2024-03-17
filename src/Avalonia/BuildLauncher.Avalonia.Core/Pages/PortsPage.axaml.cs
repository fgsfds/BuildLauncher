using Avalonia.Controls;
using BuildLauncher.ViewModels;
using Common.DI;
using Common.Enums;
using Common.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace BuildLauncher.Pages
{
    public sealed partial class PortsPage : UserControl
    {
        public PortsPage()
        {
            InitializeComponent();

            var vmFactory = BindingsManager.Provider.GetRequiredService<ViewModelsFactory>();

            Raze.DataContext = vmFactory.GetPortViewModel(PortEnum.Raze);
            NBlood.DataContext = vmFactory.GetPortViewModel(PortEnum.NBlood);
            NotBlood.DataContext = vmFactory.GetPortViewModel(PortEnum.NotBlood);
            PCExhumed.DataContext = vmFactory.GetPortViewModel(PortEnum.PCExhumed);
            RedNukem.DataContext = vmFactory.GetPortViewModel(PortEnum.RedNukem);
            EDuke32.DataContext = vmFactory.GetPortViewModel(PortEnum.EDuke32);
            BuildGDX.DataContext = vmFactory.GetPortViewModel(PortEnum.BuildGDX);
        }

        /// <summary>
        /// OpenPortsFolder
        /// </summary>
        private void OpenPortsFolderCommand(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = CommonProperties.PortsFolderPath,
                UseShellExecute = true,
            });
        }
    }
}
