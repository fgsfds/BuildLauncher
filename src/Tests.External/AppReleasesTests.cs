using Core.All.Enums;
using Core.All.Helpers;
using Core.Client.Releases;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Tests.External;

public sealed class AppReleasesTests
{
    [Fact]
    public async Task GetLatestAppReleaseTest()
    {
        using HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
        Mock<IHttpClientFactory> httpFactory = new();
        httpFactory.Setup(x => x.CreateClient(HttpClientEnum.GitHub.GetDescription())).Returns(httpClient);

        AppRepoReleasesProvider repoReleasesProvider = new(NullLogger<AppRepoReleasesProvider>.Instance, httpFactory.Object);

        var lastestRelease = await repoReleasesProvider.GetLatestReleaseAsync(AppReleaseEnum.MainApp, false);

        Assert.NotNull(lastestRelease);
        Assert.NotEmpty(lastestRelease);
    }
}
