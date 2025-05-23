using Common.Common.Serializable.Downloadable;
using Common.Enums;
using CommunityToolkit.Diagnostics;

namespace Tools.Providers;

internal sealed class ToolsRepositoriesProvider
{
    public RepositoryEntity GetToolRepo(ToolEnum toolEnum)
    {
        if (toolEnum is ToolEnum.XMapEdit)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/NoOneBlood/xmapedit/releases"),
                WindowsReleasePredicate = static x => x.FileName.EndsWith("x64.zip", StringComparison.InvariantCultureIgnoreCase),
                LinuxReleasePredicate = null
            };
        }
        else if (toolEnum is ToolEnum.Mapster32)
        {
            return new()
            {
                RepoUrl = null,
                WindowsReleasePredicate = null,
                LinuxReleasePredicate = null
            };
        }
        else
        {
            return ThrowHelper.ThrowNotSupportedException<RepositoryEntity>(toolEnum.ToString());
        }
    }
}

internal readonly struct RepositoryEntity
{
    public required Uri? RepoUrl { get; init; }
    public required Func<GitHubReleaseAsset, bool>? WindowsReleasePredicate { get; init; }
    public required Func<GitHubReleaseAsset, bool>? LinuxReleasePredicate { get; init; }
}
