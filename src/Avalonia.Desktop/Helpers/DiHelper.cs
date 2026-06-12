using Avalonia.Desktop.Misc;
using Avalonia.Desktop.ViewModels;
using Avalonia.Media.Imaging;
using Core.Client.Cache;
using Microsoft.Extensions.DependencyInjection;

namespace Avalonia.Desktop.Helpers;

public static class DiHelper
{
    /// <summary>
    /// Adds dependencies to work with MVVM.
    /// </summary>
    public static IServiceCollection WithMVVM(this IServiceCollection container)
    {
        _ = container.AddSingleton<ViewModelsFactory>();
        _ = container.AddSingleton<MainWindowViewModel>();
        return container.AddSingleton<ViewLocator>();
    }

    /// <summary>
    /// Adds dependencies to work with bitmaps cache.
    /// </summary>
    public static IServiceCollection WithBitmapsCache(this IServiceCollection container)
    {
        _ = container.AddSingleton<BitmapsCache>();
        _ = container.AddKeyedSingleton<ICacheGetter<Bitmap>>("Bitmaps", (x, _) => x.GetRequiredService<BitmapsCache>());
        return container.AddKeyedSingleton<ICacheAdder<Stream>>("Bitmaps", (x, _) => x.GetRequiredService<BitmapsCache>());
    }
}
