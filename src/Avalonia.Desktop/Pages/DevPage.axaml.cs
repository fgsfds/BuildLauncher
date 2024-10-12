using Avalonia.Controls;
using Avalonia.Desktop.ViewModels;
using Common.Client.DI;
using Microsoft.Extensions.DependencyInjection;

namespace Avalonia.Desktop.Pages;

public sealed partial class DevPage : UserControl
{
    public DevPage()
    {
        var vm = BindingsManager.Provider.GetRequiredService<DevViewModel>();

        DataContext = vm;

        InitializeComponent();
    }
}
