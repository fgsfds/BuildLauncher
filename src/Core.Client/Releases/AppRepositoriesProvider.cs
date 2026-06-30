using Core.All.Enums;
using Core.All.Helpers;
using Core.All.Interfaces;
using Core.All.Providers;

namespace Core.Client.Releases;

/// <summary>
///     Maps each <see cref="AppReleaseEnum" /> to its <see cref="RepositoryEntity" /> describing where and how to fetch releases.
/// </summary>
public sealed class AppRepositoriesProvider : IRepositoriesProvider<AppReleaseEnum>
{
    /// <inheritdoc />
    public RepositoryEntity GetRepo(AppReleaseEnum releaseEnum)
    {
        if (releaseEnum is AppReleaseEnum.MainApp)
        {
            return new()
            {
                RepoUrl = CommonConstants.GitHubReleases,
                WindowsReleasePredicate = static asset => asset.FileName.EndsWith("win-x64.zip"),
                LinuxReleasePredicate = static asset => asset.FileName.EndsWith("linux-x64.zip")
            };
        }

        throw new NotSupportedException(releaseEnum.ToString());
    }
}
