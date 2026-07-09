using System.Collections.Concurrent;
using System.Text.Json;
using Core.All.Enums;
using Core.All.Helpers;
using Core.All.Interfaces;
using Core.All.Providers;
using Core.All.Serializable.Downloadable;
using Microsoft.Extensions.Logging;

namespace Core.All.Releases;

/// <summary>
///     Base class for providers that fetch release info from GitHub or other sources.
/// </summary>
/// <typeparam name="T">Enum type identifying the entity (port, tool, app) being queried.</typeparam>
public abstract class ReleaseProviderBase<T> where T : Enum
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly ILogger _logger;

    /// <summary>
    ///     Cache of fetched releases keyed by entity enum value.
    /// </summary>
    private readonly ConcurrentDictionary<T, Dictionary<OSEnum, GeneralReleaseJsonModel>?> _releases = [];

    private readonly IRepositoriesProvider<T> _repositoriesProvider;

    /// <summary>
    ///     Cache for shared repository releases keyed by shared cache key.
    /// </summary>
    private readonly ConcurrentDictionary<string, Lazy<Task<List<GitHubReleaseJsonModel>?>>> _sharedRepoReleases = [];

    /// <summary>
    ///     Initializes a new instance of <see cref="ReleaseProviderBase{T}" />.
    /// </summary>
    /// <param name="repositoriesProvider">Provider that maps enum values to repository configurations.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    protected ReleaseProviderBase(
        IRepositoriesProvider<T> repositoriesProvider,
        ILogger logger,
        IHttpClientFactory httpClientFactory
        )
    {
        _repositoriesProvider = repositoriesProvider;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    ///     Returns the latest releases for the given entity, or <c>null</c> if none are available.
    /// </summary>
    /// <param name="e">Entity enum value identifying the release to fetch.</param>
    /// <param name="includePreReleases">If <c>true</c>, pre-release and draft entries are considered.</param>
    /// <returns>A dictionary mapping each OS to its release model, or <c>null</c> if no releases were found.</returns>
    public async Task<Dictionary<OSEnum, GeneralReleaseJsonModel>?> GetLatestReleaseAsync(T e, bool includePreReleases)
    {
        try
        {
            _logger.LogInformation($"Looking for new {e} release.");

            var repo = _repositoriesProvider.GetRepo(e);

            if (repo.RepoUrl is null)
            {
                return null;
            }

            return await FetchAndCacheReleasesAsync(e, repo, includePreReleases).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Error while getting latest release for {e}.");

            return null;
        }
    }

    /// <summary>
    ///     Processes a list of releases and extracts per-OS asset information, caching the result.
    /// </summary>
    /// <param name="key">Entity enum key for caching.</param>
    /// <param name="repo">Repository entity with asset match predicates.</param>
    /// <param name="releases">List of GitHub releases to process.</param>
    private Dictionary<OSEnum, GeneralReleaseJsonModel>? GetAndAddReleases(T key, RepositoryEntity repo, List<GitHubReleaseJsonModel> releases)
    {
        Dictionary<OSEnum, GeneralReleaseJsonModel>? result = null;
        var foundWindows = repo.WindowsReleasePredicate is null;
        var foundLinux = repo.LinuxReleasePredicate is null;

        foreach (var release in releases)
        {
            if (!foundWindows && repo.WindowsReleasePredicate is not null)
            {
                var winAss = release.Assets.FirstOrDefault(x => repo.WindowsReleasePredicate(x));

                if (winAss is not null)
                {
                    result ??= new(2);

                    result[OSEnum.Windows] = new()
                    {
                        SupportedOS = OSEnum.Windows,
                        Description = release.Description,
                        Version = GetVersion(repo, release, winAss),
                        DownloadUrl = new(winAss.DownloadUrl),
                        Hash = winAss.Digest
                    };

                    _logger.LogInformation($"Latest Windows release for {key}: {result[OSEnum.Windows].Version}.");

                    foundWindows = true;
                }
            }

            if (!foundLinux && repo.LinuxReleasePredicate is not null)
            {
                var linAss = release.Assets.FirstOrDefault(x => repo.LinuxReleasePredicate(x));

                if (linAss is not null)
                {
                    result ??= new(2);

                    result[OSEnum.Linux] = new()
                    {
                        SupportedOS = OSEnum.Linux,
                        Description = release.Description,
                        Version = GetVersion(repo, release, linAss),
                        DownloadUrl = new(linAss.DownloadUrl),
                        Hash = linAss.Digest
                    };

                    _logger.LogInformation($"Latest Linux release for {key}: {result[OSEnum.Linux].Version}.");

                    foundLinux = true;
                }
            }

            if (foundWindows && foundLinux)
            {
                break;
            }
        }

        _ = _releases.TryAdd(key, result);

        return result;
    }

    /// <summary>
    ///     Fetches and deserializes releases from the given URL, optionally filtering out pre-releases.
    /// </summary>
    /// <param name="url">GitHub releases API URL.</param>
    /// <param name="includePreReleases">Whether to include pre-release and draft entries.</param>
    private async Task<List<GitHubReleaseJsonModel>?> GetReleasesAsync(Uri url, bool includePreReleases)
    {
        using var httpClient = _httpClientFactory.CreateClient(HttpClientEnum.GitHub.GetDescription());
        await using var dataStream = await httpClient.GetStreamAsync(url).ConfigureAwait(false);

        var allReleases = await JsonSerializer.DeserializeAsync(
            dataStream,
            GitHubReleaseEntityContext.Default.ListGitHubReleaseJsonModel
            ).ConfigureAwait(false);

        if (allReleases is null)
        {
            throw new FormatException("Error while deserializing GitHub releases");
        }

        IEnumerable<GitHubReleaseJsonModel> filtered = allReleases;

        if (!includePreReleases)
        {
            filtered = filtered.Where(static x => !x.IsDraft && !x.IsPrerelease);
        }

        var result = filtered.ToList();

        result.Sort(static (a, b) =>
                        -VersionComparer.CompareVersions(a.TagName.AsSpan(), b.TagName.AsSpan())
            );

        return result;
    }

    /// <summary>
    ///     Fetches releases from a repository, using shared or per-entity caching, and extracts per-OS results.
    /// </summary>
    /// <param name="key">Entity enum key for caching.</param>
    /// <param name="repo">Repository entity configuration.</param>
    /// <param name="includePreReleases">Whether to include pre-release and draft entries.</param>
    private async Task<Dictionary<OSEnum, GeneralReleaseJsonModel>?> FetchAndCacheReleasesAsync(
        T key, RepositoryEntity repo, bool includePreReleases)
    {
        if (_releases.TryGetValue(key, out var cached))
        {
            return cached;
        }

        if (repo.CustomReleaseParser is not null)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            await using var dataStream = await httpClient.GetStreamAsync(repo.RepoUrl).ConfigureAwait(false);
            var release = repo.CustomReleaseParser(dataStream);

            _logger.LogInformation(
                release is not null
                    ? $"Latest release for {key}: {release.Version} (custom source)"
                    : $"No release found for {key} (custom source)"
                );

            Dictionary<OSEnum, GeneralReleaseJsonModel>? result = null;

            if (release is not null)
            {
                result = new()
                {
                    {
                        release.SupportedOS, release
                    }
                };
            }

            _ = _releases.TryAdd(key, result);

            return result;
        }

        List<GitHubReleaseJsonModel>? releases;

        if (repo.SharedCacheKey is not null)
        {
            var lazy = _sharedRepoReleases.GetOrAdd(
                repo.SharedCacheKey,
                _ => new(() => GetReleasesAsync(repo.RepoUrl, includePreReleases))
                );

            releases = await lazy.Value.ConfigureAwait(false);
        }
        else
        {
            releases = await GetReleasesAsync(repo.RepoUrl, includePreReleases).ConfigureAwait(false);
        }

        return releases is null ? null : GetAndAddReleases(key, repo, releases);
    }

    /// <summary>
    ///     Extracts the version string from a release, using a custom selector if available.
    /// </summary>
    /// <param name="repo">Repository entity with optional version selector.</param>
    /// <param name="release">GitHub release model.</param>
    /// <param name="asset">Matched release asset.</param>
    private static string GetVersion(RepositoryEntity repo, GitHubReleaseJsonModel release, GitHubReleaseAsset asset)
    {
        return repo.VersionSelector?.Invoke(release, asset) ?? release.TagName;
    }
}
