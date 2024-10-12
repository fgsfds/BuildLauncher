using Avalonia.Controls;
using Avalonia.Desktop.ViewModels;
using Common.Client.DI;
using Microsoft.Extensions.DependencyInjection;

namespace Avalonia.Desktop.Pages;

public sealed partial class PortsPage : UserControl
{
    public PortsPage()
    {
        InitializeComponent();

        var vmFactory = BindingsManager.Provider.GetRequiredService<ViewModelsFactory>();

        DataContext = vmFactory.GetPortsViewModel();
    }
}
