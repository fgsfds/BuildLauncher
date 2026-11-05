using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Core.Client.Helpers;

/// <summary>
///     Provides utility methods to locate the local Steam installation and retrieve active game library folders.
/// </summary>
public static class SteamHelper
{
    /// <summary>
    ///     Retrieves a list of all absolute paths to the Steam libraries on the system.
    /// </summary>
    public static IReadOnlyList<string> GetSteamLibraries()
    {
        var steamInstallPath = GetSteamInstallPath();

        if (steamInstallPath is null)
        {
            return [];
        }

        return GetSteamLibraries(steamInstallPath);
    }

    /// <summary> Finds the default library and any additional libraries in a Steam installation. </summary>
    /// <param name="steamInstallPath">The Steam installation directory.</param>
    /// <returns>The existing game-library folders.</returns>
    internal static IReadOnlyList<string> GetSteamLibraries(string steamInstallPath)
    {
        var primary = Path.Combine(steamInstallPath, "steamapps", "common");
        var result = new List<string>();
        if (Directory.Exists(primary))
        {
            result.Add(primary);
        }

        var libraryfolders = Path.Combine(steamInstallPath, "steamapps", "libraryfolders.vdf");

        try
        {
            if (File.Exists(libraryfolders))
            {
                result.AddRange(GetLibrariesFromVdf(libraryfolders));
            }
        }
        catch (IOException)
        {
            // A missing or unreadable library index must not prevent application startup.
        }
        catch (UnauthorizedAccessException)
        {
            // The default accessible library remains usable without access to the index.
        }

        return result.Distinct(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal).ToArray();
    }

    /// <summary>
    ///     Parses vdf file to extract paths to Steam libraries.
    /// </summary>
    /// <param name="pathToVdf">Path to the libraryfolders.vdf file.</param>
    internal static IReadOnlyList<string> GetLibrariesFromVdf(string pathToVdf)
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

            if (Path.IsPathFullyQualified(dir) && Directory.Exists(dir))
            {
                var path = Path.Combine(Path.GetFullPath(dir), "steamapps", "common");
                result.Add(path);
            }
        }

        return result;
    }

    /// <summary> Finds native, legacy, or Flatpak Steam data without assuming one home layout. </summary>
    /// <param name="home">The user home directory.</param>
    /// <param name="dataHome">An optional absolute XDG data directory.</param>
    /// <returns>An existing Steam installation, preferring one with a library index.</returns>
    internal static string? FindLinuxSteamInstallPath(string home, string? dataHome)
    {
        var data = !string.IsNullOrWhiteSpace(dataHome) && Path.IsPathFullyQualified(dataHome)
            ? dataHome : Path.Combine(home, ".local", "share");
        var candidates = new[]
        {
            Path.Combine(data, "Steam"),
            Path.Combine(home, ".steam", "steam"),
            Path.Combine(home, ".steam", "root"),
            Path.Combine(home, ".var", "app", "com.valvesoftware.Steam", ".local", "share", "Steam")
        };
        return candidates.FirstOrDefault(path => File.Exists(Path.Combine(path, "steamapps", "libraryfolders.vdf")))
            ?? candidates.FirstOrDefault(Directory.Exists);
    }

    /// <summary>
    ///     Retrieves the Steam installation folder path from the registry or default Linux location.
    /// </summary>
    /// <returns>The Steam install path, or null if not found.</returns>
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

            result = FindLinuxSteamInstallPath(home, Environment.GetEnvironmentVariable("XDG_DATA_HOME"));
            if (result is null)
            {
                return null;
            }
        }
        else
        {
            throw new PlatformNotSupportedException($"Can't identify Steam install path on platform: {RuntimeInformation.OSDescription}.");
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
