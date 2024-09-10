using Avalonia.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Avalonia.Desktop.DI;

public static class ViewModelsBindings
{
    public static void Load(ServiceCollection container)
    {
        container.AddSingleton<ViewModelsFactory>();

        container.AddSingleton<MainViewModel>();
        container.AddSingleton<SettingsViewModel>();
        container.AddSingleton<AboutViewModel>();
        container.AddSingleton<DevViewModel>();
    }
}
