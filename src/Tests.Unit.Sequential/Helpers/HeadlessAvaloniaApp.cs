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
                try
                {
                    var lifetime = new ClassicDesktopStyleApplicationLifetime();

                    _ = AppBuilder.Configure<Application>()
                                  .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                                  .SetupWithLifetime(lifetime);

                    lifetime.MainWindow = new Window();
                }
                catch (InvalidOperationException)
                {
                    // Avalonia's AppBuilder.Setup() uses a non-thread-safe internal flag.
                    // When xUnit v3 triggers static constructors concurrently during
                    // parallel discovery, a second thread can reach this point and throw.
                    // ViewModel tests don't need the rendering pipeline, so as long
                    // as Application.Current is set, we're fine.
                    if (Application.Current is null)
                        throw;
                }
            }

            _initialized = true;
        }
    }
}
