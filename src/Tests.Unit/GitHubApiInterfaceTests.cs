using Core.All.Enums;
using Core.All.Serializable.Downloadable;
using Core.Client.Api;
using Core.Client.Config;
using Core.Client.Releases;
using Core.Client.Tools;
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
}
