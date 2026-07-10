using System.Runtime.InteropServices;
using Core.Client;
using Core.Client.Helpers;
using Optris.Icons.Avalonia;
using Optris.Icons.Avalonia.FontAwesome7;

namespace Avalonia.Desktop;

/// <summary>
///     Application entry point.
/// </summary>
public sealed partial class Program
{
    /// <summary>
    ///     Application entry point.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>The application exit code.</returns>
    [STAThread]
    public static int Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is not Exception ex)
            {
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WinMsgBox.Show("Fatal error", ex.ToString());
            }

            SaveCrashLog();
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            SaveCrashLog();
            e.SetObserved();
        };

        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        if (File.Exists(Path.Combine(ClientProperties.WorkingFolder, ClientConsts.UpdateFile)))
        {
            AppUpdateInstaller.InstallUpdate();

            return 0;
        }

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
            var builder = BuildAvaloniaApp();

            return App.Run(builder);
        }
        catch (Exception ex)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WinMsgBox.Show(
                    "Critical error",
                    ex.ToString()
                    );
            }

            SaveCrashLog();

            return -1;
        }
    }

    /// <summary>
    ///     Builds the Avalonia application configuration.
    /// </summary>
    /// <returns>The configured AppBuilder.</returns>
    private static AppBuilder BuildAvaloniaApp()
    {
        _ = IconProvider.Current
                        .Register<FontAwesome7IconProvider>()
            ;

        return AppBuilder.Configure<App>()
                         .UsePlatformDetect()
                         .WithInterFont()
                         .LogToTrace();
    }


    /// <summary>
    ///     Saves a crash log file by copying the current log file if it exists.
    /// </summary>
    private static void SaveCrashLog()
    {
        if (File.Exists(ClientProperties.PathToLogFile))
        {
            File.Copy(
                ClientProperties.PathToLogFile,
                Path.Combine(ClientProperties.WorkingFolder, $"{DateTime.Now:dd_MM_yy_HH_mm}.crashlog"),
                true
                );
        }
    }

    /// <summary>
    ///     Provides a simple Windows message box for critical error display.
    /// </summary>
    private static partial class WinMsgBox
    {
        /// <summary>
        ///     Displays a Windows message box.
        /// </summary>
        [LibraryImport("user32.dll", EntryPoint = "MessageBoxW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int MessageBox(IntPtr hWnd, string? text, string? caption, int type);

        /// <summary>
        ///     Shows a message box with the specified title and text.
        /// </summary>
        /// <param name="title">The message box title.</param>
        /// <param name="text">The message box text.</param>
        public static void Show(string? title, string? text)
        {
            _ = MessageBox(IntPtr.Zero, text, title, 0);
        }
    }
}
