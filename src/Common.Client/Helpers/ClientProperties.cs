using CommunityToolkit.Diagnostics;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Common.Client.Helpers;

public static class ClientProperties
{
    /// <summary>
    /// Path to the folder that contains main exe
    /// </summary>
    public static readonly string WorkingFolder = Environment.CurrentDirectory;

    /// <summary>
    /// Path to the data folder
    /// </summary>
    public static readonly string DataFolderPath = Path.Combine(WorkingFolder, "Data");

    /// <summary>
    /// Path to the ports folder
    /// </summary>
    public static readonly string PortsFolderPath = Path.Combine(DataFolderPath, "Ports");

    /// <summary>
    /// Path to the tools folder
    /// </summary>
    public static readonly string ToolsFolderPath = Path.Combine(DataFolderPath, "Tools");

    /// <summary>
    /// Path to the saved games folder
    /// </summary>
    public static readonly string SavedGamesFolderPath = Path.Combine(DataFolderPath, "Saves");

    /// <summary>
    /// Is app running in the developer mode
    /// </summary>
    public static bool IsDeveloperMode { get; set; } = false;

    /// <summary>
    /// Is app running in offline mode
    /// </summary>
    public static bool IsOfflineMode { get; set; }

    /// <summary>
    /// Current app version
    /// </summary>
    public static Version CurrentVersion => Assembly.GetEntryAssembly()?.GetName().Version ?? ThrowHelper.ThrowArgumentNullException<Version>();

    /// <summary>
    /// Name of the executable file
    /// </summary>
    public static string ExecutableName
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Process.GetCurrentProcess().MainModule?.ModuleName ?? "BuildLauncher.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return AppDomain.CurrentDomain.FriendlyName;
            }
            else
            {
                ThrowHelper.ThrowPlatformNotSupportedException();
                return string.Empty;
            }
        }
    }
}
