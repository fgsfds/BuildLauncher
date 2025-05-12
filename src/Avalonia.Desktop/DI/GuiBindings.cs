using Avalonia.Desktop.Helpers;
using Avalonia.Desktop.Misc;
using Avalonia.Desktop.ViewModels;
using Avalonia.Media.Imaging;
using Common.Client.Cache;
using Microsoft.Extensions.DependencyInjection;

namespace Avalonia.Desktop.DI;

public static class GuiBindings
{
    public static void Load(ServiceCollection container)
    {
        _ = container.AddSingleton<ViewModelsFactory>();

        _ = container.AddSingleton<MainWindowViewModel>();

        _ = container.AddSingleton<ViewLocator>();

        _ = container.AddSingleton<BitmapsCache>();
        _ = container.AddKeyedSingleton<ICacheGetter<Bitmap>>("Bitmaps", (x, _) => x.GetRequiredService<BitmapsCache>());
        _ = container.AddKeyedSingleton<ICacheAdder<Stream>>("Bitmaps", (x, _) => x.GetRequiredService<BitmapsCache>());

        _ = container.AddSingleton<CachedHashToBitmapConverter>();
    }
}
