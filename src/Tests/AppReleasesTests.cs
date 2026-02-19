using Common.All.Enums;
using Common.All.Helpers;
using Common.All.Providers;
using Moq;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Tests;

public sealed class AppReleasesTests
{
    [Fact]
    public async Task DeserializeAddonJsonAsync()
    {
        using HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
        var logger = new Mock<ILogger>().Object;
        Mock<IHttpClientFactory> httpFactory = new();
        httpFactory.Setup(x => x.CreateClient(HttpClientEnum.GitHub.GetDescription())).Returns(httpClient);

        RepoAppReleasesProvider releasesProvider = new(logger, httpFactory.Object);

        var lastestRelease = await releasesProvider.GetLatestReleaseAsync(false);

        Assert.NotNull(lastestRelease);
        Assert.NotEmpty(lastestRelease);
    }
}









