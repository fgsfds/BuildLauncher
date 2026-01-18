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
    public static readonly string TempFolderPath = Path.Combine(DataFolderPath, "Temp");

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
    public static Version CurrentVersion => Assembly.GetEntryAssembly()?.GetName().Version ?? throw new ArgumentNullException();
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
                throw new PlatformNotSupportedException();
            }
        }
    }

    /// <summary>
    /// Path to local addons.json
    /// </summary>
    public static string? PathToLocalAddonsJson
    {
        get
        {
            var path1 = Path.Combine(WorkingFolder, @"..\..\..\..\db\addons.json");

            if (File.Exists(path1))
            {
                return path1;
            }

            var path2 = Path.Combine(WorkingFolder, @"db\addons.json");

            if (File.Exists(path2))
            {
                return path2;
            }

            return null;
        }
    }

    /// <summary>
    /// Path to local data.json
    /// </summary>
    public static string? PathToLocalDataJson
    {
        get
        {
            var path1 = Path.Combine(WorkingFolder, @"..\..\..\..\db\data.json");

            if (File.Exists(path1))
            {
                return path1;
            }

            var path2 = Path.Combine(WorkingFolder, @"db\data.json");

            if (File.Exists(path2))
            {
                return path2;
            }

            return null;
        }
    }

    public static string PathToLogFile => Path.Combine(WorkingFolder, "BuildLauncher.log");
}
