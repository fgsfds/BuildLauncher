using Core.All.Enums;
using Core.All.Providers;
using Ports.Releases;

namespace Tests.Unit;

public sealed class PortsRepositoriesProviderTests
{
    private readonly PortsRepositoriesProvider _provider = new();

    [Fact]
    public void GetRepo_BuildGDX_ReturnsCorrectRepo()
    {
        var repo = _provider.GetRepo(PortEnum.BuildGDX);
        Assert.NotNull(repo.RepoUrl);
        Assert.Contains("fgsfds/BuildGDX-Releases", repo.RepoUrl!.ToString());
        Assert.NotNull(repo.WindowsReleasePredicate);
        Assert.Null(repo.LinuxReleasePredicate);
    }

    [Fact]
    public void GetRepo_Raze_ReturnsWithBothPredicates()
    {
        var repo = _provider.GetRepo(PortEnum.Raze);
        Assert.NotNull(repo.WindowsReleasePredicate);
        Assert.NotNull(repo.LinuxReleasePredicate);
        Assert.Contains("ZDoom/Raze", repo.RepoUrl!.ToString());
    }

    [Fact]
    public void GetRepo_EDuke32_HasCustomParserAndNoPredicates()
    {
        var repo = _provider.GetRepo(PortEnum.EDuke32);
        Assert.NotNull(repo.RepoUrl);
        Assert.Null(repo.WindowsReleasePredicate);
        Assert.Null(repo.LinuxReleasePredicate);
        Assert.NotNull(repo.CustomReleaseParser);
    }

    [Fact]
    public void GetRepo_VoidSW_ReturnsNullRepoUrl()
    {
        var repo = _provider.GetRepo(PortEnum.VoidSW);
        Assert.Null(repo.RepoUrl);
        Assert.Null(repo.WindowsReleasePredicate);
        Assert.Null(repo.LinuxReleasePredicate);
    }

    [Fact]
    public void GetRepo_NBlood_HasSharedCacheKey()
    {
        var repo = _provider.GetRepo(PortEnum.NBlood);
        Assert.Equal("nukeykt/NBlood", repo.SharedCacheKey);
        Assert.Contains("nukeykt/NBlood", repo.RepoUrl!.ToString());
    }

    [Fact]
    public void GetRepo_PCExhumed_SharesCacheWithNBlood()
    {
        var repo = _provider.GetRepo(PortEnum.PCExhumed);
        Assert.Equal("nukeykt/NBlood", repo.SharedCacheKey);
    }

    [Fact]
    public void GetRepo_RedNukem_SharesCacheWithNBlood()
    {
        var repo = _provider.GetRepo(PortEnum.RedNukem);
        Assert.Equal("nukeykt/NBlood", repo.SharedCacheKey);
    }

    [Fact]
    public void GetRepo_NotBlood_HasVersionSelector()
    {
        var repo = _provider.GetRepo(PortEnum.NotBlood);
        Assert.NotNull(repo.VersionSelector);
        Assert.Contains("clipmove/NotBlood", repo.RepoUrl!.ToString());
    }

    [Fact]
    public void GetRepo_Fury_ReturnsNullRepoUrl()
    {
        var repo = _provider.GetRepo(PortEnum.Fury);
        Assert.Null(repo.RepoUrl);
    }

    [Fact]
    public void GetRepo_DosBox_HasCompoundPredicate()
    {
        var repo = _provider.GetRepo(PortEnum.DosBox);
        Assert.NotNull(repo.WindowsReleasePredicate);
        Assert.Null(repo.LinuxReleasePredicate);
        Assert.Contains("dosbox-staging/dosbox-staging", repo.RepoUrl!.ToString());
    }

    [Fact]
    public void GetRepo_ZeroRecomp_HasCustomParser()
    {
        var repo = _provider.GetRepo(PortEnum.ZeroRecomp);
        Assert.NotNull(repo.RepoUrl);
        Assert.NotNull(repo.CustomReleaseParser);
        Assert.Null(repo.WindowsReleasePredicate);
    }

    [Fact]
    public void GetRepo_StubNotSupported_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>((Action)(() => _provider.GetRepo(PortEnum.Stub)));
    }

    [Fact]
    public void GetRepo_AllPortEnums_ReturnNonNullExceptExcluded()
    {
        var allEnums = Enum.GetValues<PortEnum>();
        foreach (var portEnum in allEnums)
        {
            if (portEnum is PortEnum.Stub)
            {
                Assert.Throws<ArgumentOutOfRangeException>((Action)(() => _provider.GetRepo(portEnum)));
            }
            else
            {
                var repo = _provider.GetRepo(portEnum);
                Assert.NotNull(repo);
            }
        }
    }
}
