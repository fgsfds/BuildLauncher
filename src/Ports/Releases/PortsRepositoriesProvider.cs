using System.Text.RegularExpressions;
using Core.All.Enums;
using Core.All.Interfaces;
using Core.All.Providers;
using Core.All.Serializable.Downloadable;

namespace Ports.Releases;

/// <summary>
///     Maps each <see cref="PortEnum" /> to its <see cref="RepositoryEntity" /> describing where and how to fetch releases.
///     Contains the EDuke32 HTML-scraping parser as a custom release source.
/// </summary>
public sealed partial class PortsRepositoriesProvider : IRepositoriesProvider<PortEnum>
{
    /// <summary>
    ///     Returns the repository configuration for the specified port.
    /// </summary>
    /// <param name="releaseEnum">Target port.</param>
    /// <returns>A <see cref="RepositoryEntity" /> describing the release source and matching rules.</returns>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="releaseEnum" /> has no associated repository.</exception>
    public RepositoryEntity GetRepo(PortEnum releaseEnum)
    {
        if (releaseEnum is PortEnum.BuildGDX)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/fgsfds/BuildGDX-Releases/releases"),
                WindowsReleasePredicate = static x => x.FileName.EndsWith("windows.zip", StringComparison.OrdinalIgnoreCase),
                LinuxReleasePredicate = null
            };
        }

        if (releaseEnum is PortEnum.Raze)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/ZDoom/Raze/releases"),
                WindowsReleasePredicate = static x => x.FileName.EndsWith("windows.zip", StringComparison.OrdinalIgnoreCase),
                LinuxReleasePredicate = static x => x.FileName.EndsWith("linux-portable.tar.xz", StringComparison.OrdinalIgnoreCase)
            };
        }

        if (releaseEnum is PortEnum.EDuke32)
        {
            return new()
            {
                RepoUrl = new("https://dukeworld.com/eduke32/synthesis/latest/"),
                WindowsReleasePredicate = null,
                LinuxReleasePredicate = null,
                CustomReleaseParser = ParseEDuke32Release
            };
        }

        if (releaseEnum is PortEnum.VoidSW)
        {
            return new()
            {
                RepoUrl = null,
                WindowsReleasePredicate = null,
                LinuxReleasePredicate = null
            };
        }

        if (releaseEnum is PortEnum.NBlood)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/nukeykt/NBlood/releases"),
                WindowsReleasePredicate = static x => x.FileName.StartsWith("nblood_win64", StringComparison.OrdinalIgnoreCase),
                LinuxReleasePredicate = null,
                SharedCacheKey = "nukeykt/NBlood"
            };
        }

        if (releaseEnum is PortEnum.NotBlood)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/clipmove/NotBlood/releases"),
                WindowsReleasePredicate = static x => x.FileName.EndsWith("win64.zip", StringComparison.OrdinalIgnoreCase),
                LinuxReleasePredicate = static x => x.FileName.EndsWith("linux-clang.zip", StringComparison.OrdinalIgnoreCase),
                VersionSelector = static (_, asset) => asset.UpdatedDate.ToUniversalTime().ToString()
            };
        }

        if (releaseEnum is PortEnum.PCExhumed)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/nukeykt/NBlood/releases"),
                WindowsReleasePredicate = static x => x.FileName.StartsWith("pcexhumed_win64", StringComparison.OrdinalIgnoreCase),
                LinuxReleasePredicate = null,
                SharedCacheKey = "nukeykt/NBlood"
            };
        }

        if (releaseEnum is PortEnum.RedNukem)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/nukeykt/NBlood/releases"),
                WindowsReleasePredicate = static x => x.FileName.StartsWith("rednukem_win64", StringComparison.OrdinalIgnoreCase),
                LinuxReleasePredicate = null,
                SharedCacheKey = "nukeykt/NBlood"
            };
        }

        if (releaseEnum is PortEnum.Fury)
        {
            return new()
            {
                RepoUrl = null,
                WindowsReleasePredicate = null,
                LinuxReleasePredicate = null
            };
        }

        if (releaseEnum is PortEnum.DosBox)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/dosbox-staging/dosbox-staging/releases"),
                WindowsReleasePredicate = static x => x.FileName.StartsWith("dosbox-staging-windows-x64", StringComparison.OrdinalIgnoreCase) && x.FileName.EndsWith("zip", StringComparison.OrdinalIgnoreCase),
                LinuxReleasePredicate = null
            };
        }

        if (releaseEnum is PortEnum.ZeroRecomp)
        {
            return new()
            {
                RepoUrl = new("https://dnzh-overclocked.com/"),
                WindowsReleasePredicate = null,
                LinuxReleasePredicate = null,
                CustomReleaseParser = ParseZeroHourRelease
            };
        }

        throw new ArgumentOutOfRangeException(nameof(releaseEnum), releaseEnum, $"Unsupported release: {releaseEnum}.");
    }

    /// <summary>
    ///     Parses the EDuke32 release HTML page to find the Windows release.
    /// </summary>
    /// <param name="responseStream">HTML response stream.</param>
    private static GeneralReleaseJsonModel? ParseEDuke32Release(Stream responseStream) => ParseHtmlRelease(responseStream, EDuke32WindowsReleaseRegex(), "win64", EDuke32VersionRegex(), "r", null);

    /// <summary>
    ///     Parses the Zero Hour Overclocked release HTML page to find the Windows release.
    /// </summary>
    /// <param name="responseStream">HTML response stream.</param>
    private static GeneralReleaseJsonModel? ParseZeroHourRelease(Stream responseStream) => ParseHtmlRelease(responseStream, ZeroHourWindowsReleaseRegex(), "windows-x64-full", ZeroHourVersionRegex(), null, "https://dnzh-overclocked.com/");

    /// <summary>
    ///     Parses an HTML release page to extract a download URL and version.
    /// </summary>
    /// <param name="responseStream">HTML response stream.</param>
    /// <param name="fileNameRegex">Regex to match the file name.</param>
    /// <param name="lineFilter">Substring filter for HTML lines.</param>
    /// <param name="versionRegex">Regex to extract the version from the file name.</param>
    /// <param name="versionPrefix">Optional prefix to prepend to the version.</param>
    /// <param name="baseUrl">Optional base URL for the download link.</param>
    private static GeneralReleaseJsonModel? ParseHtmlRelease(
        Stream responseStream, Regex fileNameRegex, string lineFilter,
        Regex versionRegex, string? versionPrefix, string? baseUrl)
    {
        string? matchedFileName = null;

        using (var reader = new StreamReader(responseStream))
        {
            while (reader.ReadLine() is
                   { } line)
            {
                if (!line.Contains(lineFilter))
                {
                    continue;
                }

                var match = fileNameRegex.Match(line);

                if (match.Success)
                {
                    matchedFileName = match.Value;

                    break;
                }
            }
        }

        if (matchedFileName is null)
        {
            return null;
        }

        var versionMatch = versionRegex.Match(matchedFileName);

        if (!versionMatch.Success)
        {
            return null;
        }

        return new()
        {
            SupportedOS = OSEnum.Windows,
            Description = string.Empty,
            Version = versionPrefix + versionMatch.Value,
            DownloadUrl = new(baseUrl + matchedFileName),
            Hash = null
        };
    }

    /// <summary>
    ///     Regex to match EDuke32 Windows release file names in HTML.
    /// </summary>
    [GeneratedRegex("(?<=\")[^\"]*eduke32_win64_2[^\"]*(?=\")")]
    private static partial Regex EDuke32WindowsReleaseRegex();

    /// <summary>
    ///     Regex to match Zero Hour Overclocked Windows release file names in HTML.
    /// </summary>
    [GeneratedRegex("(?<=\")[^\"]*windows-x64-full[^\"]*(?=\")")]
    private static partial Regex ZeroHourWindowsReleaseRegex();

    /// <summary>
    ///     Regex to extract the version number from EDuke32 release file names.
    /// </summary>
    [GeneratedRegex(@"(?<=\-)(\d*)(?=\-)")]
    private static partial Regex EDuke32VersionRegex();

    /// <summary>
    ///     Regex to extract the version number from Zero Hour Overclocked release file names.
    /// </summary>
    [GeneratedRegex(@"\d+\.\d+\.\d+")]
    private static partial Regex ZeroHourVersionRegex();
}
