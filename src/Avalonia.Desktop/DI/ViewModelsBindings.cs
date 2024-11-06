using Avalonia.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Avalonia.Desktop.DI;

public static class ViewModelsBindings
{
    public static void Load(ServiceCollection container)
    {
        _ = container.AddSingleton<ViewModelsFactory>();

        _ = container.AddSingleton<MainViewModel>();
    }
}
