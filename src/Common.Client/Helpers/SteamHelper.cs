using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Common.Client.Helpers;

/// <summary>
/// Provides utility methods to locate the local Steam installation and retrieve active game library folders.
/// </summary>
public static class SteamHelper
{
    /// <summary>
    /// Retrieves a list of all absolute paths to the Steam libraries on the system.
    /// </summary>
    public static List<string> GetSteamLibraries()
    {
        var steamInstallPath = GetSteamInstallPath();

        if (steamInstallPath is null)
        {
            return [];
        }

        var libraryfolders = Path.Combine(steamInstallPath, "steamapps", "libraryfolders.vdf");

        if (!File.Exists(libraryfolders))
        {
            return [];
        }

        return GetLibratiesFromVdf(libraryfolders);
    }

    /// <summary>
    /// Parses vdf file to extract paths to Steam libraries.
    /// </summary>
    /// <param name="pathToVdf">Path to the libraryfolders.vdf file.</param>
    internal static List<string> GetLibratiesFromVdf(string pathToVdf)
    {
        List<string> result = new(4);

        foreach (var line in File.ReadLines(pathToVdf))
        {
            var span = line.AsSpan();

            if (!span.Contains("\"path\"", StringComparison.Ordinal))
            {
                continue;
            }

            var lastQuote = span.LastIndexOf('"');
            if (lastQuote <= 0)
            {
                continue;
            }

            var secondToLastQuote = span[..lastQuote].LastIndexOf('"');
            if (secondToLastQuote < 0 || secondToLastQuote >= lastQuote - 1)
            {
                continue;
            }

            var dirSpan = span[(secondToLastQuote + 1)..lastQuote].Trim();

            var dir = dirSpan.ToString().Replace("\\\\", "\\");
            if (Directory.Exists(dir))
            {
                var path = Path.Combine(dir, "steamapps", "common");
                result.Add(path);
            }
        }

        return result;
    }

    /// <summary>
    /// Locates the root Steam installation directory.
    /// </summary>
    private static string? GetSteamInstallPath()
    {
        string? result;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var path = (string?)Registry
            .GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam", "SteamPath", null);

            if (path is null)
            {
                //Logger.Error("Can't find Steam install folder");
                return null;
            }

            result = path.Replace('/', Path.DirectorySeparatorChar);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            result = Path.Combine(home, ".local/share/Steam");
        }
        else
        {
            throw new PlatformNotSupportedException("Can't identify platform");
        }

        if (!Directory.Exists(result))
        {
            //Logger.Error($"Steam install folder {result} doesn't exist");
            return null;
        }

        //Logger.Info($"Steam install folder is {result}");
        return result;
    }
}
