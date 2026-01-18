using Common.All.Enums;
using Common.All.Serializable.Downloadable;

namespace Tools.Providers;

internal static class ToolsRepositoriesProvider
{
    public static RepositoryEntity GetToolRepo(ToolEnum toolEnum)
    {
        if (toolEnum is ToolEnum.XMapEdit)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/NoOneBlood/xmapedit/releases"),
                WindowsReleasePredicate = static x => x.FileName.EndsWith("x64.zip", StringComparison.OrdinalIgnoreCase),
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
        else if (toolEnum is ToolEnum.DOSBlood)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/clipmove/DOSBlood/releases"),
                WindowsReleasePredicate = static x => x.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase),
                LinuxReleasePredicate = null
            };
        }
        else
        {
            throw new NotSupportedException(toolEnum.ToString());
        }
    }
}

internal readonly struct RepositoryEntity
{
    public required Uri? RepoUrl { get; init; }
    public required Func<GitHubReleaseAsset, bool>? WindowsReleasePredicate { get; init; }
    public required Func<GitHubReleaseAsset, bool>? LinuxReleasePredicate { get; init; }
}
