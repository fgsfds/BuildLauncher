using System.Text.Json;
using System.Text.RegularExpressions;
using Common.Common.Interfaces;
using Common.Common.Serializable.Downloadable;
using Common.Enums;
using Common.Helpers;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Ports.Providers;

internal sealed partial class PortsReleasesRepoRetriever : IRetriever<Dictionary<PortEnum, GeneralReleaseJsonModel>?>
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;

    public PortsReleasesRepoRetriever(
        ILogger logger,
        HttpClient httpClient
        )
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<Dictionary<PortEnum, GeneralReleaseJsonModel>?> RetrieveAsync()
    {
        _logger.LogInformation("Looking for new ports releases");

        var ports = Enum.GetValues<PortEnum>();
        Dictionary<PortEnum, GeneralReleaseJsonModel>? result = [];

        foreach (var port in ports)
        {
            if (port is PortEnum.Stub or PortEnum.Fury)
            {
                continue;
            }

            try
            {
                var releases = await GetLatestReleaseAsync(port).ConfigureAwait(false);

                if (releases is not null)
                {
                    if (releases.TryGetValue(CommonProperties.OSEnum, out var winRelease))
                    {
                        result.AddOrReplace(port, winRelease);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Error while getting latest release for {port}");
            }
        }

        return result;
    }

    /// <summary>
    /// Get the latest release of the selected port
    /// </summary>
    /// <param name="portEnum">Port</param>
    private async Task<Dictionary<OSEnum, GeneralReleaseJsonModel>?> GetLatestReleaseAsync(PortEnum portEnum)
    {
        var repo = PortsRepositoriesProvider.GetPortRepo(portEnum);

        Dictionary<OSEnum, GeneralReleaseJsonModel>? result = null;

        if (repo.RepoUrl is null)
        {
            return null;
        }

        if (portEnum is PortEnum.VoidSW)
        {
            return null;
        }

        var response = await _httpClient.GetStringAsync(repo.RepoUrl).ConfigureAwait(false);

        if (portEnum is PortEnum.EDuke32)
        {
            var edukeRelease = EDuke32Hack(response);
            return edukeRelease is null ? null : new() { { OSEnum.Windows, edukeRelease } };
        }

        var allReleases = JsonSerializer.Deserialize(response, GitHubReleaseEntityContext.Default.ListGitHubReleaseJsonModel)
            ?? ThrowHelper.ThrowFormatException<List<GitHubReleaseJsonModel>>("Error while deserializing GitHub releases");

        var releases = allReleases.Where(static x => !x.IsDraft && !x.IsPrerelease);

        if (releases is null)
        {
            return null;
        }

        if (repo.WindowsReleasePredicate is not null)
        {
            foreach (var release in releases)
            {
                var winAss = release.Assets.FirstOrDefault(x => repo.WindowsReleasePredicate(x));

                if (winAss is not null)
                {
                    GeneralReleaseJsonModel portRelease = new()
                    {
                        SupportedOS = OSEnum.Windows,
                        Description = release.Description,
                        Version = GetVersion(portEnum, release, winAss),
                        DownloadUrl = winAss is null ? null : new(winAss.DownloadUrl),
                    };

                    result ??= [];
                    result.Add(OSEnum.Windows, portRelease);

                    _logger.LogInformation($"Latest Windows release for {portEnum}: {portRelease.Version}");

                    break;
                }
            }
        }

        if (repo.LinuxReleasePredicate is not null)
        {
            foreach (var release in releases)
            {
                var linAss = release.Assets.FirstOrDefault(x => repo.LinuxReleasePredicate(x));

                if (linAss is not null)
                {
                    GeneralReleaseJsonModel portRelease = new()
                    {
                        SupportedOS = OSEnum.Linux,
                        Description = release.Description,
                        Version = GetVersion(portEnum, release, linAss),
                        DownloadUrl = linAss is null ? null : new(linAss.DownloadUrl),
                    };

                    result ??= [];
                    result.Add(OSEnum.Linux, portRelease);

                    _logger.LogInformation($"Latest Linux release for {portEnum}: {portRelease.Version}");

                    break;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Get port version
    /// </summary>
    private string GetVersion(PortEnum portEnum, GitHubReleaseJsonModel release, GitHubReleaseAsset asset)
    {
        if (portEnum is PortEnum.NotBlood)
        {
            return asset.UpdatedDate.ToUniversalTime().ToString();
        }

        return release.TagName;
    }

    /// <summary>
    /// Hack to get EDuke32 release since dukeworld doesn't have API
    /// </summary>
    /// <param name="response">Json response</param>
    private GeneralReleaseJsonModel? EDuke32Hack(string response)
    {
        var regex = EDuke32WindowsReleaseRegex();
        var fileName = regex.Matches(response).FirstOrDefault();

        if (fileName is null)
        {
            return null;
        }

        var regexVersion = EDuke32VersionRegex();
        var version = regexVersion.Matches(fileName.ToString()).FirstOrDefault();

        if (version is null)
        {
            return null;
        }

        GeneralReleaseJsonModel release = new()
        {
            SupportedOS = OSEnum.Windows,
            Description = string.Empty,
            Version = "r" + version,
            DownloadUrl = new($"https://dukeworld.com/eduke32/synthesis/latest/{fileName}")
        };

        _logger.LogInformation($"Latest release for {nameof(PortEnum.EDuke32)}: {release.Version}");

        return release;
    }

    [GeneratedRegex("eduke32_win64_2[^\"]*")]
    private partial Regex EDuke32WindowsReleaseRegex();

    [GeneratedRegex(@"(?<=\-)(\d*)(?=\-)")]
    private partial Regex EDuke32VersionRegex();
}
