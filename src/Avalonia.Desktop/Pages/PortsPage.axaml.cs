using Avalonia.Controls;
using Avalonia.Desktop.ViewModels;
using Common.Client.Helpers;
using Common.DI;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Avalonia.Desktop.Pages;

public sealed partial class PortsPage : UserControl
{
    public PortsPage()
    {
        InitializeComponent();

        var vmFactory = BindingsManager.Provider.GetRequiredService<ViewModelsFactory>();

        DataContext = vmFactory.GetPortsViewModel();
    }

    /// <summary>
    /// OpenPortsFolder
    /// </summary>
    private void OpenPortsFolderCommand(object? sender, Interactivity.RoutedEventArgs e)
    {
        _ = Process.Start(new ProcessStartInfo
        {
            FileName = ClientProperties.PortsFolderPath,
            UseShellExecute = true,
        });
    }
}
