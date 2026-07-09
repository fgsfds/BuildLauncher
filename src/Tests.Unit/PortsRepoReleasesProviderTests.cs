using System.Net;
using Core.All.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Ports.Releases;

namespace Tests.Unit;

/// <summary>
///     Tests for the <see cref="PortsRepoReleasesProvider" /> class.
/// </summary>
public sealed class PortsRepoReleasesProviderTests
{
    private const string JsonResponse = """
        [
          {
            "tag_name": "v1.0.0",
            "draft": false,
            "prerelease": false,
            "assets": [
              {
                "name": "nblood_win64.zip",
                "browser_download_url": "https://github.com/nukeykt/NBlood/releases/download/v1.0.0/nblood_win64.zip",
                "updated_at": "2025-01-01T00:00:00Z"
              },
              {
                "name": "pcexhumed_win64.zip",
                "browser_download_url": "https://github.com/nukeykt/NBlood/releases/download/v1.0.0/pcexhumed_win64.zip",
                "updated_at": "2025-01-01T00:00:00Z"
              },
              {
                "name": "rednukem_win64.zip",
                "browser_download_url": "https://github.com/nukeykt/NBlood/releases/download/v1.0.0/rednukem_win64.zip",
                "updated_at": "2025-01-01T00:00:00Z"
              }
            ],
            "body": "Initial release"
          },
          {
            "tag_name": "v1.2.0",
            "draft": false,
            "prerelease": false,
            "assets": [
              {
                "name": "nblood_win64.zip",
                "browser_download_url": "https://github.com/nukeykt/NBlood/releases/download/v1.2.0/nblood_win64.zip",
                "updated_at": "2025-01-01T00:00:00Z"
              },
              {
                "name": "pcexhumed_win64.zip",
                "browser_download_url": "https://github.com/nukeykt/NBlood/releases/download/v1.2.0/pcexhumed_win64.zip",
                "updated_at": "2025-01-01T00:00:00Z"
              },
              {
                "name": "rednukem_win64.zip",
                "browser_download_url": "https://github.com/nukeykt/NBlood/releases/download/v1.2.0/rednukem_win64.zip",
                "updated_at": "2025-01-01T00:00:00Z"
              }
            ],
            "body": "Initial release"
          },
          {
            "tag_name": "v1.1.0",
            "draft": false,
            "prerelease": false,
            "assets": [
              {
                "name": "nblood_win64.zip",
                "browser_download_url": "https://github.com/nukeykt/NBlood/releases/download/v1.1.0/nblood_win64.zip",
                "updated_at": "2025-01-01T00:00:00Z"
              },
              {
                "name": "pcexhumed_win64.zip",
                "browser_download_url": "https://github.com/nukeykt/NBlood/releases/download/v1.1.0/pcexhumed_win64.zip",
                "updated_at": "2025-01-01T00:00:00Z"
              },
              {
                "name": "rednukem_win64.zip",
                "browser_download_url": "https://github.com/nukeykt/NBlood/releases/download/v1.1.0/rednukem_win64.zip",
                "updated_at": "2025-01-01T00:00:00Z"
              }
            ],
            "body": "Initial release"
          },
          {
            "tag_name": "v1.4.0",
            "draft": false,
            "prerelease": true,
            "assets": [
              {
                "name": "nblood_win64.zip",
                "browser_download_url": "https://github.com/nukeykt/NBlood/releases/download/v1.4.0/nblood_win64.zip",
                "updated_at": "2025-01-01T00:00:00Z"
              },
              {
                "name": "pcexhumed_win64.zip",
                "browser_download_url": "https://github.com/nukeykt/NBlood/releases/download/v1.4.0/pcexhumed_win64.zip",
                "updated_at": "2025-01-01T00:00:00Z"
              },
              {
                "name": "rednukem_win64.zip",
                "browser_download_url": "https://github.com/nukeykt/NBlood/releases/download/v1.4.0/rednukem_win64.zip",
                "updated_at": "2025-01-01T00:00:00Z"
              }
            ],
            "body": "Initial release"
          }
        ]
        """;

    private const string RazeJsonResponse = """
        [
          {
            "tag_name": "v1.0.0",
            "draft": false,
            "prerelease": false,
            "assets": [
              {
                "name": "raze-1.0.0-windows.zip",
                "browser_download_url": "https://github.com/ZDoom/Raze/releases/download/v1.0.0/raze-1.0.0-windows.zip",
                "updated_at": "2025-01-15T00:00:00Z"
              },
              {
                "name": "raze-1.0.0-linux-portable.tar.xz",
                "browser_download_url": "https://github.com/ZDoom/Raze/releases/download/v1.0.0/raze-1.0.0-linux-portable.tar.xz",
                "updated_at": "2025-01-15T00:00:00Z"
              }
            ],
            "body": "Initial release"
          }
        ]
        """;

    /// <summary>
    ///     Tests that the shared cache key causes GetReleasesAsync to be called only once.
    /// </summary>
    [Fact]
    public async Task GetLatestReleaseAsync_SharedCacheKey_CallsGetReleasesAsyncOnlyOnce()
    {
        using var httpClient = new HttpClient(new FakeHttpMessageHandler());
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var provider = new PortsRepoReleasesProvider(
            NullLogger<PortsRepoReleasesProvider>.Instance,
            httpFactoryMock.Object
            );

        _ = await provider.GetLatestReleaseAsync(PortEnum.NBlood, false);
        _ = await provider.GetLatestReleaseAsync(PortEnum.PCExhumed, false);
        _ = await provider.GetLatestReleaseAsync(PortEnum.RedNukem, false);

        httpFactoryMock.Verify(
            x => x.CreateClient(It.IsAny<string>()),
            Times.Once
            );
    }

    /// <summary>
    ///     Tests that getting the latest stable release returns the correct version.
    /// </summary>
    [Fact]
    public async Task GetLatestReleaseAsync_StableOnly_ReturnsLatestRelease()
    {
        using var httpClient = new HttpClient(new FakeHttpMessageHandler());
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var provider = new PortsRepoReleasesProvider(
            NullLogger<PortsRepoReleasesProvider>.Instance,
            httpFactoryMock.Object
            );

        var nBloodReleases = await provider.GetLatestReleaseAsync(PortEnum.NBlood, false);

        Assert.NotNull(nBloodReleases);
        var nBlood = Assert.Single(nBloodReleases);
        Assert.Equal("v1.2.0", nBlood.Value.Version);
    }

    /// <summary>
    ///     Tests that getting the latest release with pre-releases enabled returns the pre-release version.
    /// </summary>
    [Fact]
    public async Task GetLatestReleaseAsync_PreReleasesEnabled_ReturnsLatestPreRelease()
    {
        using var httpClient = new HttpClient(new FakeHttpMessageHandler());
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var provider = new PortsRepoReleasesProvider(
            NullLogger<PortsRepoReleasesProvider>.Instance,
            httpFactoryMock.Object
            );

        var nBloodReleases = await provider.GetLatestReleaseAsync(PortEnum.NBlood, true);

        Assert.NotNull(nBloodReleases);
        var nBlood = Assert.Single(nBloodReleases);
        Assert.Equal("v1.4.0", nBlood.Value.Version);
    }

    /// <summary>
    ///     Tests that when the repo URL is null, null is returned.
    /// </summary>
    [Fact]
    public async Task GetLatestReleaseAsync_RepoUrlIsNull_ReturnsNull()
    {
        using var httpClient = new HttpClient(new FakeHttpMessageHandler());
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var provider = new PortsRepoReleasesProvider(
            NullLogger<PortsRepoReleasesProvider>.Instance,
            httpFactoryMock.Object
            );

        var result = await provider.GetLatestReleaseAsync(PortEnum.VoidSW, false);

        Assert.Null(result);
    }

    /// <summary>
    ///     Tests that when an HTTP error occurs, null is returned.
    /// </summary>
    [Fact]
    public async Task GetLatestReleaseAsync_HttpError_ReturnsNull()
    {
        var httpFactoryMock = new Mock<IHttpClientFactory>();

        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
                       .Throws<HttpRequestException>();

        var provider = new PortsRepoReleasesProvider(
            NullLogger<PortsRepoReleasesProvider>.Instance,
            httpFactoryMock.Object
            );

        var result = await provider.GetLatestReleaseAsync(PortEnum.NBlood, false);

        Assert.Null(result);
    }

    /// <summary>
    ///     Tests that an unsupported port enum returns null.
    /// </summary>
    [Fact]
    public async Task GetLatestReleaseAsync_UnsupportedPortEnum_ReturnsNull()
    {
        using var httpClient = new HttpClient(new FakeHttpMessageHandler());
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var provider = new PortsRepoReleasesProvider(
            NullLogger<PortsRepoReleasesProvider>.Instance,
            httpFactoryMock.Object
            );

        var result = await provider.GetLatestReleaseAsync(PortEnum.Stub, false);

        Assert.Null(result);
    }

    /// <summary>
    ///     Tests that both Linux and Windows assets are returned when available.
    /// </summary>
    [Fact]
    public async Task GetLatestReleaseAsync_LinuxAndWindowsAssets_ReturnsBoth()
    {
        using var httpClient = new HttpClient(new FakeRazeHttpMessageHandler());
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var provider = new PortsRepoReleasesProvider(
            NullLogger<PortsRepoReleasesProvider>.Instance,
            httpFactoryMock.Object
            );

        var releases = await provider.GetLatestReleaseAsync(PortEnum.Raze, false);

        Assert.NotNull(releases);
        Assert.Equal(2, releases.Count);
        Assert.True(releases.ContainsKey(OSEnum.Windows));
        Assert.True(releases.ContainsKey(OSEnum.Linux));
        Assert.Equal("v1.0.0", releases[OSEnum.Windows].Version);
        Assert.Equal("v1.0.0", releases[OSEnum.Linux].Version);
    }

    /// <summary>
    ///     Tests that the version selector returns a custom version.
    /// </summary>
    [Fact]
    public async Task GetLatestReleaseAsync_VersionSelector_ReturnsCustomVersion()
    {
        using var httpClient = new HttpClient(new FakeHttpMessageHandler());
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var provider = new PortsRepoReleasesProvider(
            NullLogger<PortsRepoReleasesProvider>.Instance,
            httpFactoryMock.Object
            );

        var releases = await provider.GetLatestReleaseAsync(PortEnum.NotBlood, false);

        Assert.NotNull(releases);
        var release = Assert.Single(releases);
        Assert.NotEqual("v1.2.0", release.Value.Version);
        Assert.Contains("2025", release.Value.Version);
    }

    /// <summary>
    ///     Fake HTTP message handler for testing with predefined JSON responses.
    /// </summary>
    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonResponse)
            };

            return Task.FromResult(response);
        }
    }


    /// <summary>
    ///     Fake HTTP message handler for testing Raze-specific JSON responses.
    /// </summary>
    private sealed class FakeRazeHttpMessageHandler : HttpMessageHandler
    {
        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(RazeJsonResponse)
            };

            return Task.FromResult(response);
        }
    }
}
