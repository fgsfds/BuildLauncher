using Core.All.Enums;
using Core.All.Serializable.Downloadable;
using Core.Client.Releases;

namespace Tests.Unit;

public sealed class AppRepositoriesProviderTests
{
    private readonly AppRepositoriesProvider _provider = new();

    [Fact]
    public void GetRepo_MainApp_ReturnsWithCorrectPredicates()
    {
        var repo = _provider.GetRepo(AppReleaseEnum.MainApp);
        Assert.NotNull(repo.RepoUrl);
        Assert.NotNull(repo.WindowsReleasePredicate);
        Assert.NotNull(repo.LinuxReleasePredicate);
        Assert.Null(repo.SharedCacheKey);
    }

    [Fact]
    public void GetRepo_WindowsPredicate_MatchesWinX64Zip()
    {
        var repo = _provider.GetRepo(AppReleaseEnum.MainApp);
        var asset = new GitHubReleaseAsset
        {
            FileName = "buildlauncher-1.0.0-win-x64.zip"
        };
        Assert.True(repo.WindowsReleasePredicate!(asset));
    }

    [Fact]
    public void GetRepo_LinuxPredicate_MatchesLinuxX64Zip()
    {
        var repo = _provider.GetRepo(AppReleaseEnum.MainApp);
        var asset = new GitHubReleaseAsset
        {
            FileName = "buildlauncher-1.0.0-linux-x64.zip"
        };
        Assert.True(repo.LinuxReleasePredicate!(asset));
    }
}
