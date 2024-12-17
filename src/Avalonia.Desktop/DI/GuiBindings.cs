using Avalonia.Desktop.ViewModels;
using Avalonia.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Avalonia.Desktop.DI;

public static class GuiBindings
{
    public static void Load(ServiceCollection container)
    {
        _ = container.AddSingleton<ViewModelsFactory>();

        _ = container.AddSingleton<MainViewModel>();

        _ = container.AddSingleton<MainWindow>();
    }
}
