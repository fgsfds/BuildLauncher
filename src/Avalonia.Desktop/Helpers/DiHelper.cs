using Avalonia.Desktop.Misc;
using Avalonia.Desktop.Services;
using Avalonia.Desktop.ViewModels;
using Avalonia.Media.Imaging;
using Core.Client.Cache;
using Core.Client.Enums;
using Core.Client.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Avalonia.Desktop.Helpers;

/// <summary>
///     Configures dependency injection services for the desktop project.
/// </summary>
public static class DiHelper
{
    /// <summary>
    ///     Adds dependencies to work with MVVM.
    /// </summary>
    public static IServiceCollection WithMVVM(this IServiceCollection container)
    {
        _ = container.AddSingleton<IViewModelsFactory, ViewModelsFactory>();
        _ = container.AddSingleton<MainWindowViewModel>();

        _ = container.AddSingleton<ViewLocator>();
        _ = container.AddSingleton<IFolderOpener, FolderOpener>();
        _ = container.AddSingleton<IUserNotifier, UserNotifier>();

        return container;
    }

    /// <summary>
    ///     Adds dependencies to work with bitmaps cache.
    /// </summary>
    public static IServiceCollection WithBitmapsCache(this IServiceCollection container)
    {
        _ = container.AddSingleton<BitmapsCache>();
        _ = container.AddKeyedSingleton<ICacheGetter<Bitmap>>(KeyedServicesEnum.Bitmaps, (x, _) => x.GetRequiredService<BitmapsCache>());

        return container.AddKeyedSingleton<ICacheAdder<Stream>>(KeyedServicesEnum.Bitmaps, (x, _) => x.GetRequiredService<BitmapsCache>());
    }
}
