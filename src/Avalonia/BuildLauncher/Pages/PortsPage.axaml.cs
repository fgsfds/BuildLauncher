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

            var vmFactory = BindingsManager.Provider.GetRequiredService<PortViewModelFactory>();

            Raze.DataContext = vmFactory.Create(PortEnum.Raze);
            NBlood.DataContext = vmFactory.Create(PortEnum.NBlood);
            NotBlood.DataContext = vmFactory.Create(PortEnum.NotBlood);
            PCExhumed.DataContext = vmFactory.Create(PortEnum.PCExhumed);
            RedNukem.DataContext = vmFactory.Create(PortEnum.RedNukem);
            EDuke32.DataContext = vmFactory.Create(PortEnum.EDuke32);
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
