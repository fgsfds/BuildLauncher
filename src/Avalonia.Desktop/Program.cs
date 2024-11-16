﻿using Common.Client.Helpers;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.MaterialDesign;
using System.Runtime.ExceptionServices;

namespace Avalonia.Desktop;

public sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Contains("--dev"))
        {
            ClientProperties.IsDeveloperMode = true;
        }
        if (args.Contains("--offline"))
        {
            ClientProperties.IsOfflineMode = true;
        }
        try
        {
            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            _ = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            ExceptionDispatchInfo.Capture(ex).Throw();
        }
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (!ClientProperties.IsDeveloperMode)
        {
            var exe = Path.Combine(ClientProperties.WorkingFolder, ClientProperties.ExecutableName);
            var args = "--crash " + $@"""{e.ExceptionObject}""";

            _ = System.Diagnostics.Process.Start(exe, args);
        }

        Environment.FailFast(string.Empty);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        _ = IconProvider.Current
            .Register<MaterialDesignIconProvider>()
            ;

        return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
