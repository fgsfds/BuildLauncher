using Core.All.Enums;
using Core.All.Interfaces;
using Core.All.Providers;

namespace Tools.Releases;

/// <summary>
///     Maps each <see cref="ToolEnum" /> to its <see cref="RepositoryEntity" /> describing where and how to fetch releases.
/// </summary>
public sealed class ToolsRepositoriesProvider : IRepositoriesProvider<ToolEnum>
{
    /// <summary>
    ///     Returns the repository configuration for the specified tool.
    /// </summary>
    /// <param name="releaseEnum">Target tool.</param>
    /// <returns>A <see cref="RepositoryEntity" /> describing the release source and matching rules.</returns>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="releaseEnum" /> has no associated repository.</exception>
    public RepositoryEntity GetRepo(ToolEnum releaseEnum)
    {
        if (releaseEnum is ToolEnum.XMapEdit)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/NoOneBlood/xmapedit/releases"),
                WindowsReleasePredicate = static x => x.FileName.EndsWith("x64.zip", StringComparison.OrdinalIgnoreCase),
                LinuxReleasePredicate = null,
                VersionSelector = static (_, asset) => asset.UpdatedDate.ToUniversalTime().ToString()
            };
        }

        if (releaseEnum is ToolEnum.Mapster32)
        {
            return new()
            {
                RepoUrl = null,
                WindowsReleasePredicate = null,
                LinuxReleasePredicate = null
            };
        }

        if (releaseEnum is ToolEnum.DOSBlood)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/clipmove/DOSBlood/releases"),
                WindowsReleasePredicate = static x => x.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase),
                LinuxReleasePredicate = null,
                VersionSelector = static (_, asset) => asset.UpdatedDate.ToUniversalTime().ToString()
            };
        }

        throw new NotSupportedException(releaseEnum.ToString());
    }
}
