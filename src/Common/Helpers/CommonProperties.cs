﻿using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Common.Helpers
{
    public static class CommonProperties
    {
        /// <summary>
        /// Path to the folder that contains main exe
        /// </summary>
        public static readonly string ExeFolderPath = Directory.GetCurrentDirectory();

        /// <summary>
        /// Path to the data folder
        /// </summary>
        public static readonly string DataFolderPath = Path.Combine(ExeFolderPath, "Data");

        /// <summary>
        /// Path to the ports folder
        /// </summary>
        public static readonly string PortsFolderPath = Path.Combine(DataFolderPath, "Ports");

        /// <summary>
        /// Is app running in the developer mode
        /// </summary>
        public static bool IsDevMode { get; set; } = false;

        /// <summary>
        /// Current app version
        /// </summary>
        public static Version? CurrentVersion => Assembly.GetEntryAssembly()?.GetName().Version;

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
                    ThrowHelper.PlatformNotSupportedException();
                    return string.Empty;
                }
            }
        }
    }
}
