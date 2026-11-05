using System.Runtime.InteropServices;
using Core.Client;
using Core.Client.Helpers;
using Microsoft.Extensions.Logging;
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
        var smokeTest = args.Contains("--smoke-test") || args.Contains("--smoke-test-restart");

        if (smokeTest && (!args.Contains("--offline") || !File.Exists(Path.Combine(AppContext.BaseDirectory, ".published-smoke-test"))))
        {
            return 2;
        }

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is not Exception ex)
            {
                return;
            }

            if (!smokeTest && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WinMsgBox.Show("Fatal error", ex.ToString());
            }

            SaveCrashLog();
            if (smokeTest)
            {
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "smoke-error.txt"), ex.ToString());
            }
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            if (e.Exception is null)
            {
                return;
            }

            using var loggerFactory = LoggerFactory.Create(x => x.AddDebug());
            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogCritical(e.Exception, "Unobserved task exception");

            SaveCrashLog();
            if (smokeTest)
            {
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "smoke-error.txt"), e.Exception.ToString());
            }

            e.SetObserved();
        };

        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        AppUpdateInstaller.CleanupCompletedUpdates(ClientProperties.WorkingFolder);

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

            return App.Run(builder, smokeTest, args.Contains("--smoke-test-restart"));
        }
        catch (Exception ex)
        {
            if (!smokeTest && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WinMsgBox.Show(
                    "Critical error",
                    ex.ToString()
                    );
            }

            SaveCrashLog();

            if (smokeTest)
            {
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "smoke-error.txt"), ex.ToString());
            }

            return -1;
        }
    }

    /// <summary>
    ///     Builds the Avalonia application configuration.
    /// </summary>
    /// <returns>The configured AppBuilder.</returns>
    private static AppBuilder BuildAvaloniaApp()
    {
        _ = IconProvider.Current!
                        .Register<FontAwesome7IconProvider>()
            ;

        return AppBuilder.Configure<App>()
                         .UsePlatformDetect()
                         .WithInterFont()!
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
                Path.Combine(ClientProperties.WorkingFolder, $"{DateTime.Now:dd_MM_yy_HH_mm_ss}.crashlog"),
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
