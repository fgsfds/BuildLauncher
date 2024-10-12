using Avalonia.Controls;
using Avalonia.Desktop.ViewModels;
using Common.Client.DI;
using Common.Client.Helpers;
using Common.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Avalonia.Desktop.Pages;

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
    private void OpenToolsFolderCommand(object? sender, Interactivity.RoutedEventArgs e)
    {
        _ = Process.Start(new ProcessStartInfo
        {
            FileName = ClientProperties.ToolsFolderPath,
            UseShellExecute = true,
        });
    }
}
