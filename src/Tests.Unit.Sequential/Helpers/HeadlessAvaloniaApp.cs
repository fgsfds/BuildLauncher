using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;

namespace Tests.Unit.Helpers;

internal static class HeadlessAvaloniaApp
{
    private static readonly object _lock = new();
    private static volatile bool _initialized;

    public static void EnsureInitialized()
    {
        if (_initialized)
            return;

        lock (_lock)
        {
            if (_initialized)
                return;

            if (Application.Current is null)
            {
                var lifetime = new ClassicDesktopStyleApplicationLifetime();

                _ = AppBuilder.Configure<Application>()
                              .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                              .SetupWithLifetime(lifetime);

                lifetime.MainWindow = new Window();
            }

            _initialized = true;
        }
    }
}
