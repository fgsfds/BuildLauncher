using System.Text.RegularExpressions;
using Common.All.Enums;
using Common.All.Providers;
using Common.All.Serializable.Downloadable;
using Microsoft.Extensions.Logging;

namespace Ports.Providers;

public sealed partial class PortsReleasesProvider : ReleaseProvider<PortEnum>
{
    private static readonly SemaphoreSlim _semaphore = new(1);
    private static volatile List<GitHubReleaseJsonModel>? _cachedNBloodReleases;

    public PortsReleasesProvider(
        ILogger logger,
        IHttpClientFactory httpClientFactory
        ) : base(logger, httpClientFactory)
    {
    }

    /// <summary>
    /// Get the latest release of the selected port
    /// </summary>
    /// <param name="portEnum">Port</param>
    public override async Task<Dictionary<OSEnum, GeneralReleaseJsonModel>?> GetLatestReleaseAsync(PortEnum portEnum)
    {
        try
        {
            _logger.LogInformation($"Looking for new {portEnum} release.", portEnum);

            var repo = PortsRepositoriesProvider.GetPortRepo(portEnum);
            if (repo.RepoUrl is null)
            {
                return null;
            }

            if (_releases.TryGetValue(portEnum, out var existingRelease))
            {
                return existingRelease;
            }

            if (portEnum is PortEnum.EDuke32)
            {
                using var httpClient = _httpClientFactory.CreateClient();
                using var dataStream = await httpClient.GetStreamAsync(repo.RepoUrl).ConfigureAwait(false);
                var edukeRelease = EDuke32Hack(dataStream);

                return edukeRelease is null ? null : new() { { OSEnum.Windows, edukeRelease } };
            }

            var isSharedNBloodCache = portEnum is PortEnum.NBlood or PortEnum.PCExhumed or PortEnum.RedNukem;

            if (isSharedNBloodCache)
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);

                try
                {
                    if (_cachedNBloodReleases is null)
                    {
                        var fetchedReleases = await GetReleasesAsync(repo.RepoUrl).ConfigureAwait(false);
                        if (fetchedReleases is null)
                        {
                            return null;
                        }

                        _cachedNBloodReleases = fetchedReleases;
                    }

                    return GetAndAddReleases(portEnum, repo, _cachedNBloodReleases);
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            var releases = await GetReleasesAsync(repo.RepoUrl).ConfigureAwait(false);
            return releases is null ? null : GetAndAddReleases(portEnum, repo, releases);
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
    protected override string GetVersion(PortEnum portEnum, GitHubReleaseJsonModel release, GitHubReleaseAsset asset)
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
    /// <param name="responseStream">Json stream.</param>
    private GeneralReleaseJsonModel? EDuke32Hack(Stream responseStream)
    {
        var windowsRegex = EDuke32WindowsReleaseRegex();
        string? matchedFileName = null;

        using (var reader = new StreamReader(responseStream))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!line.Contains("win64"))
                {
                    continue;
                }

                var match = windowsRegex.Match(line);
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

        if (matchedFileName.Contains('/'))
        {
            matchedFileName = matchedFileName.Substring(matchedFileName.LastIndexOf('/') + 1);
        }
        matchedFileName = matchedFileName.Trim('"').Trim('\'');

        var regexVersion = EDuke32VersionRegex();
        var versionMatch = regexVersion.Match(matchedFileName);

        if (!versionMatch.Success)
        {
            return null;
        }

        GeneralReleaseJsonModel release = new()
        {
            SupportedOS = OSEnum.Windows,
            Description = string.Empty,
            Version = "r" + versionMatch.Value,
            DownloadUrl = new($"https://dukeworld.com/eduke32/synthesis/latest/{matchedFileName}"),
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
