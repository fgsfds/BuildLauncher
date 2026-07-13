using Core.All.Enums;
using Tools.Releases;

namespace Tests.Unit;

public sealed class ToolsRepositoriesProviderTests
{
    private readonly ToolsRepositoriesProvider _provider = new();

    [Fact]
    public void GetRepo_XMapEdit_ReturnsCorrectRepo()
    {
        var repo = _provider.GetRepo(ToolEnum.XMapEdit);
        Assert.NotNull(repo.RepoUrl);
        Assert.Contains("NoOneBlood/xmapedit", repo.RepoUrl!.ToString());
        Assert.NotNull(repo.WindowsReleasePredicate);
        Assert.Null(repo.LinuxReleasePredicate);
        Assert.NotNull(repo.VersionSelector);
    }

    [Fact]
    public void GetRepo_Mapster32_ReturnsNullRepoUrl()
    {
        var repo = _provider.GetRepo(ToolEnum.Mapster32);
        Assert.Null(repo.RepoUrl);
        Assert.Null(repo.WindowsReleasePredicate);
        Assert.Null(repo.LinuxReleasePredicate);
    }

    [Fact]
    public void GetRepo_DOSBlood_ReturnsCorrectRepo()
    {
        var repo = _provider.GetRepo(ToolEnum.DOSBlood);
        Assert.NotNull(repo.RepoUrl);
        Assert.Contains("clipmove/DOSBlood", repo.RepoUrl!.ToString());
        Assert.NotNull(repo.WindowsReleasePredicate);
        Assert.Null(repo.LinuxReleasePredicate);
        Assert.NotNull(repo.VersionSelector);
    }

    [Fact]
    public void GetRepo_AllToolEnums_ReturnNonNullExceptExcluded()
    {
        var allEnums = Enum.GetValues<ToolEnum>();
        foreach (var toolEnum in allEnums)
        {
            var repo = _provider.GetRepo(toolEnum);
            Assert.NotNull(repo);
        }
    }
}
