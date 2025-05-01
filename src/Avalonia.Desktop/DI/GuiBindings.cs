using Avalonia.Desktop.Misc;
using Avalonia.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Avalonia.Desktop.DI;

public static class GuiBindings
{
    public static void Load(ServiceCollection container)
    {
        _ = container.AddSingleton<ViewModelsFactory>();

        _ = container.AddSingleton<MainWindowViewModel>();

        _ = container.AddSingleton<ViewLocator>();
    }
}
