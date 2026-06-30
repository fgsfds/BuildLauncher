using Core.All.Serializable.Downloadable;

namespace Core.All.Providers;

/// <summary>
///     Describes a source repository: where to fetch releases from and how to match assets per OS.
/// </summary>
public readonly struct RepositoryEntity
{
    /// <summary>
    ///     URL to the releases API (GitHub) or custom source. <c>null</c> means no releases available.
    /// </summary>
    public required Uri? RepoUrl { get; init; }

    /// <summary>
    ///     Predicate to identify the Windows asset within a release's asset list. <c>null</c> if Windows releases are not supported.
    /// </summary>
    public required Func<GitHubReleaseAsset, bool>? WindowsReleasePredicate { get; init; }

    /// <summary>
    ///     Predicate to identify the Linux asset within a release's asset list. <c>null</c> if Linux releases are not supported.
    /// </summary>
    public required Func<GitHubReleaseAsset, bool>? LinuxReleasePredicate { get; init; }

    /// <summary>
    ///     Custom parser for non-GitHub sources. Takes the response stream and returns a release model, or <c>null</c>.
    /// </summary>
    public Func<Stream, GeneralReleaseJsonModel?>? CustomReleaseParser { get; init; }

    /// <summary>
    ///     Custom function to extract the version string from a release and its matched asset. Falls back to <c>release.TagName</c> when <c>null</c>.
    /// </summary>
    public Func<GitHubReleaseJsonModel, GitHubReleaseAsset, string>? VersionSelector { get; init; }

    /// <summary>
    ///     Shared cache key for repos used by multiple entities (e.g., NBlood/PCExhumed/RedNukem sharing the same GitHub repo).
    ///     When set, releases are fetched once and reused across all entities with the same key.
    /// </summary>
    public string? SharedCacheKey { get; init; }
}
