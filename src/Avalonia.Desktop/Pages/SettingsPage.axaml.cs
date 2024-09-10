using Avalonia.Controls;
using Avalonia.Desktop.ViewModels;
using Common.DI;
using Microsoft.Extensions.DependencyInjection;

namespace Avalonia.Desktop.Pages;

public sealed partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        var vm = BindingsManager.Provider.GetRequiredService<SettingsViewModel>();

        DataContext = vm;

        InitializeComponent();
    }
}
