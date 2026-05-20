using Common.All.Serializable.Downloadable;

namespace Common.All;

public readonly struct RepositoryEntity
{
    public required Uri? RepoUrl { get; init; }
    public required Func<GitHubReleaseAsset, bool>? WindowsReleasePredicate { get; init; }
    public required Func<GitHubReleaseAsset, bool>? LinuxReleasePredicate { get; init; }
}
