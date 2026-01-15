using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using Common.All.Enums;
using Common.All.Helpers;
using Common.All.Interfaces;
using Common.All.Serializable.Downloadable;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Ports.Providers;

public sealed partial class PortsReleasesProvider : IReleaseProvider<PortEnum>
{
    private readonly ConcurrentDictionary<PortEnum, Dictionary<OSEnum, GeneralReleaseJsonModel>?> _releases = [];

    private static readonly Lock _lock = new();
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private volatile string? _nBloodCache;

    public PortsReleasesProvider(
        ILogger logger,
        IHttpClientFactory httpClientFactory
        )
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Get the latest release of the selected port
    /// </summary>
    /// <param name="portEnum">Port</param>
    public async Task<Dictionary<OSEnum, GeneralReleaseJsonModel>?> GetLatestReleaseAsync(PortEnum portEnum)
    {
        try
        {
            _logger.LogInformation($"Looking for new {portEnum} release.");

            var repo = PortsRepositoriesProvider.GetPortRepo(portEnum);

            if (repo.RepoUrl is null)
            {
                return null;
            }

            if (_releases.TryGetValue(portEnum, out var existingRelease))
            {
                return existingRelease;
            }

            string? data;

            if (portEnum is PortEnum.EDuke32)
            {
                using var httpClient = _httpClientFactory.CreateClient();
                data = await httpClient.GetStringAsync(repo.RepoUrl).ConfigureAwait(false);
                var edukeRelease = EDuke32Hack(data);
                return edukeRelease is null ? null : new() { { OSEnum.Windows, edukeRelease } };
            }

            if (portEnum is PortEnum.NBlood or PortEnum.PCExhumed or PortEnum.RedNukem
                && _nBloodCache is not null)
            {
                using (_lock.EnterScope())
                {
                    data = _nBloodCache;
                }
            }
            else
            {
                using var httpClient = _httpClientFactory.CreateClient(HttpClientEnum.GitHub.GetDescription());
                using var response = await httpClient.GetAsync(repo.RepoUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                _ = response.EnsureSuccessStatusCode();
                data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (portEnum is PortEnum.NBlood or PortEnum.PCExhumed or PortEnum.RedNukem)
                {
                    using (_lock.EnterScope())
                    {
                        _nBloodCache = data;
                    }
                }
            }

            var allReleases = JsonSerializer.Deserialize(data, GitHubReleaseEntityContext.Default.ListGitHubReleaseJsonModel)
                ?? ThrowHelper.ThrowFormatException<List<GitHubReleaseJsonModel>>("Error while deserializing GitHub releases");

            var releases = allReleases
                .Where(static x => !x.IsDraft && !x.IsPrerelease)
                .ToList();

            if (releases is null)
            {
                return null;
            }

            Dictionary<OSEnum, GeneralReleaseJsonModel>? result = null;

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
                            DownloadUrl = new(winAss.DownloadUrl),
                            Hash = winAss.Digest
                        };

                        result ??= [];
                        result.Add(OSEnum.Windows, portRelease);

                        _logger.LogInformation($"Latest Windows release for {portEnum}: {portRelease.Version}.");

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
                            DownloadUrl = new(linAss.DownloadUrl),
                            Hash = linAss.Digest
                        };

                        result ??= [];
                        result.Add(OSEnum.Linux, portRelease);

                        _logger.LogInformation($"Latest Linux release for {portEnum}: {portRelease.Version}.");

                        break;
                    }
                }
            }

            _ = _releases.TryAdd(portEnum, result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Error while getting latest release for {portEnum}.");
            return null;
        }
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
            DownloadUrl = new($"https://dukeworld.com/eduke32/synthesis/latest/{fileName}"),
            Hash = null
        };

        _logger.LogInformation($"Latest Windows release for {nameof(PortEnum.EDuke32)}: {release.Version}");

        return release;
    }

    [GeneratedRegex("eduke32_win64_2[^\"]*")]
    private partial Regex EDuke32WindowsReleaseRegex();

    [GeneratedRegex(@"(?<=\-)(\d*)(?=\-)")]
    private partial Regex EDuke32VersionRegex();
}
