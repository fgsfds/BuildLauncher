using System.Net;
using System.Text.Json;
using Core.All.Enums;
using Core.All.Serializable.Downloadable;
using Core.Client.Api;
using Core.Client.Releases;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Ports.Releases;
using Tools.Releases;

namespace Tests.Unit;

public sealed class GitHubApiInterfaceTests
{
    [Fact]
    public async Task GetLatestPortReleaseAsync_DelegatesToPortsProvider()
    {
        var portsRepoProvider = new PortsRepoReleasesProvider(NullLogger<PortsRepoReleasesProvider>.Instance, new Mock<IHttpClientFactory>().Object);
        var toolsRepoProvider = new ToolsRepoReleasesProvider(NullLogger<ToolsRepoReleasesProvider>.Instance, new Mock<IHttpClientFactory>().Object);
        var appRepoProvider = new AppRepoReleasesProvider(NullLogger<AppRepoReleasesProvider>.Instance, new Mock<IHttpClientFactory>().Object);

        var api = new GitHubApiInterface(
            portsRepoProvider,
            toolsRepoProvider,
            appRepoProvider,
            new Mock<IHttpClientFactory>().Object,
            NullLogger<GitHubApiInterface>.Instance
        );

        var result = await api.GetLatestPortReleaseAsync(PortEnum.Stub);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetLatestToolReleaseAsync_DelegatesToToolsProvider()
    {
        var portsRepoProvider = new PortsRepoReleasesProvider(NullLogger<PortsRepoReleasesProvider>.Instance, new Mock<IHttpClientFactory>().Object);
        var toolsRepoProvider = new ToolsRepoReleasesProvider(NullLogger<ToolsRepoReleasesProvider>.Instance, new Mock<IHttpClientFactory>().Object);
        var appRepoProvider = new AppRepoReleasesProvider(NullLogger<AppRepoReleasesProvider>.Instance, new Mock<IHttpClientFactory>().Object);

        var api = new GitHubApiInterface(
            portsRepoProvider,
            toolsRepoProvider,
            appRepoProvider,
            new Mock<IHttpClientFactory>().Object,
            NullLogger<GitHubApiInterface>.Instance
        );

        var result = await api.GetLatestToolReleaseAsync(ToolEnum.Mapster32);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetLatestAppReleaseAsync_DelegatesToAppProvider()
    {
        var portsRepoProvider = new PortsRepoReleasesProvider(NullLogger<PortsRepoReleasesProvider>.Instance, new Mock<IHttpClientFactory>().Object);
        var toolsRepoProvider = new ToolsRepoReleasesProvider(NullLogger<ToolsRepoReleasesProvider>.Instance, new Mock<IHttpClientFactory>().Object);
        var appRepoProvider = new AppRepoReleasesProvider(NullLogger<AppRepoReleasesProvider>.Instance, new Mock<IHttpClientFactory>().Object);

        var api = new GitHubApiInterface(
            portsRepoProvider,
            toolsRepoProvider,
            appRepoProvider,
            new Mock<IHttpClientFactory>().Object,
            NullLogger<GitHubApiInterface>.Instance
        );

        var result = await api.GetLatestAppReleaseAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAddonsAsync_TransientHttpError_RetriesAndSucceeds()
    {
        var json = CreateAddonsJson();
        var callCount = 0;

        var handler = new TestHandler((_, ct) =>
        {
            var call = Interlocked.Increment(ref callCount);
            if (call <= 2)
            {
                return Task.FromException<HttpResponseMessage>(new HttpRequestException("Simulated failure"));
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
        });

        var api = CreateApi(handler);

        var result = await api.GetAddonsAsync(GameEnum.Duke3D);

        Assert.NotNull(result);
        var addon = Assert.Single(result);
        Assert.Equal("Duke Addon", addon.Title);
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task GetAddonsAsync_AllGamesReceiveAddons_AfterSingleSuccessfulFetch()
    {
        var json = CreateAddonsJson();
        var callCount = 0;

        var handler = new TestHandler((_, _) =>
        {
            Interlocked.Increment(ref callCount);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
        });

        var api = CreateApi(handler);

        var dukeResult = await api.GetAddonsAsync(GameEnum.Duke3D);
        var bloodResult = await api.GetAddonsAsync(GameEnum.Blood);
        var wangResult = await api.GetAddonsAsync(GameEnum.Wang);

        Assert.NotNull(dukeResult);
        Assert.NotNull(bloodResult);
        Assert.NotNull(wangResult);

        var dukeAddon = Assert.Single(dukeResult);
        Assert.Equal("Duke Addon", dukeAddon.Title);

        var bloodAddon = Assert.Single(bloodResult);
        Assert.Equal("Blood Addon", bloodAddon.Title);

        var wangAddon = Assert.Single(wangResult);
        Assert.Equal("SW Addon", wangAddon.Title);

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task GetAddonsAsync_FormatException_DoesNotRetry()
    {
        var handler = new TestHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null")
            })
        );

        var api = CreateApi(handler);

        var result = await api.GetAddonsAsync(GameEnum.Duke3D);

        Assert.Null(result);
    }

    private static GitHubApiInterface CreateApi(HttpMessageHandler handler)
    {
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(handler));

        var portsRepoProvider = new PortsRepoReleasesProvider(NullLogger<PortsRepoReleasesProvider>.Instance, new Mock<IHttpClientFactory>().Object);
        var toolsRepoProvider = new ToolsRepoReleasesProvider(NullLogger<ToolsRepoReleasesProvider>.Instance, new Mock<IHttpClientFactory>().Object);
        var appRepoProvider = new AppRepoReleasesProvider(NullLogger<AppRepoReleasesProvider>.Instance, new Mock<IHttpClientFactory>().Object);

        return new GitHubApiInterface(
            portsRepoProvider,
            toolsRepoProvider,
            appRepoProvider,
            httpFactoryMock.Object,
            NullLogger<GitHubApiInterface>.Instance
        );
    }

    private static string CreateAddonsJson()
    {
        var data = new Dictionary<GameEnum, List<DownloadableAddonJsonModel>>
        {
            [GameEnum.Duke3D] =
            [
                new()
                {
                    Id = "addon1",
                    Version = "1.0",
                    Title = "Duke Addon",
                    AddonType = AddonTypeEnum.Mod,
                    Game = GameEnum.Duke3D,
                    FileSize = 1000,
                    DownloadUrl = new Uri("https://example.com/1.zip"),
                    IsDisabled = false
                }
            ],
            [GameEnum.Blood] =
            [
                new()
                {
                    Id = "addon2",
                    Version = "1.0",
                    Title = "Blood Addon",
                    AddonType = AddonTypeEnum.Map,
                    Game = GameEnum.Blood,
                    FileSize = 2000,
                    DownloadUrl = new Uri("https://example.com/2.zip"),
                    IsDisabled = false
                }
            ],
            [GameEnum.Wang] =
            [
                new()
                {
                    Id = "addon3",
                    Version = "1.0",
                    Title = "SW Addon",
                    AddonType = AddonTypeEnum.TC,
                    Game = GameEnum.Wang,
                    FileSize = 3000,
                    DownloadUrl = new Uri("https://example.com/3.zip"),
                    IsDisabled = false
                }
            ]
        };

        return JsonSerializer.Serialize(data, DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel);
    }

    private sealed class TestHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

        public TestHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) => _handler = handler;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _handler(request, cancellationToken);
    }
}
