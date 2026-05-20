using System.Collections.Concurrent;
using System.Text.Json;
using Core.All.Helpers;
using Core.All.Enums;
using Core.All.Serializable.Downloadable;
using Microsoft.Extensions.Logging;

namespace Core.All.Providers;

public abstract class ReleaseProvider<T> where T : Enum
{
    protected readonly ConcurrentDictionary<T, Dictionary<OSEnum, GeneralReleaseJsonModel>?> _releases = [];
    protected readonly ILogger _logger;
    protected readonly IHttpClientFactory _httpClientFactory;

    public ReleaseProvider(
        ILogger logger,
        IHttpClientFactory httpClientFactory
        )
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }


    public abstract Task<Dictionary<OSEnum, GeneralReleaseJsonModel>?> GetLatestReleaseAsync(T e);

    protected Dictionary<OSEnum, GeneralReleaseJsonModel>? GetAndAddReleases(T portEnum, RepositoryEntity repo, List<GitHubReleaseJsonModel> releases)
    {
        Dictionary<OSEnum, GeneralReleaseJsonModel>? result = new(2);

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

    protected async Task<List<GitHubReleaseJsonModel>?> GetReleasesAsync(Uri url)
    {
        using var httpClient = _httpClientFactory.CreateClient(HttpClientEnum.GitHub.GetDescription());
        using var dataStream = await httpClient.GetStreamAsync(url).ConfigureAwait(false);

        var allReleases = await JsonSerializer.DeserializeAsync(
            dataStream,
            GitHubReleaseEntityContext.Default.ListGitHubReleaseJsonModel
            ).ConfigureAwait(false);

        if (allReleases is null)
        {
            throw new FormatException("Error while deserializing GitHub releases");
        }

        var releases = allReleases
            .Where(static x => !x.IsDraft && !x.IsPrerelease)
            .ToList();

        if (releases is null)
        {
            return null;
        }

        return releases;
    }
    protected abstract string GetVersion(T toolEnum, GitHubReleaseJsonModel release, GitHubReleaseAsset asset);
}
