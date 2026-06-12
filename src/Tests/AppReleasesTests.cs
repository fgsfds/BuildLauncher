using Core.All.Enums;
using Core.All.Helpers;
using Core.All.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Tests;

public sealed class AppReleasesTests
{
    [Fact]
    public async Task DeserializeAddonJsonAsync()
    {
        using HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
        Mock<IHttpClientFactory> httpFactory = new();
        httpFactory.Setup(x => x.CreateClient(HttpClientEnum.GitHub.GetDescription())).Returns(httpClient);

        RepoAppReleasesProvider releasesProvider = new(NullLogger<RepoAppReleasesProvider>.Instance, httpFactory.Object);

        var lastestRelease = await releasesProvider.GetLatestReleaseAsync(false);

        Assert.NotNull(lastestRelease);
        Assert.NotEmpty(lastestRelease);
    }
}
