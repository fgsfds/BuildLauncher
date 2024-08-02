using Avalonia;
using ClientCommon.Helpers;
using Projektanker.Icons.Avalonia.FontAwesome;
using Projektanker.Icons.Avalonia;

namespace BuildLauncher.Desktop;

public sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Contains("-dev"))
        {
            ClientProperties.IsDevMode = true;
        }

        BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current
            .Register<FontAwesomeIconProvider>()
            ;

        return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}
