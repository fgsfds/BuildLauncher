using Core.All.Enums;
using Core.All.Helpers;
using Core.Client.Api;
using Core.Client.Config;
using Core.Client.Releases;
using Core.Client.Tools;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Ports.Installer;
using Ports.Ports;
using Ports.Ports.EDuke32;
using Ports.Releases;
using Tools.Releases;

namespace Tests.External;

/// <summary>
///     Tests for port installation.
/// </summary>
public sealed class PortsInstallerTests
{
    /// <summary>
    ///     Gets theory data with all ports to test installation.
    /// </summary>
    public static IEnumerable<TheoryDataRow<BasePort>> GetPorts()
    {
        yield return new EDuke32();
        yield return new Raze();
        yield return new NBlood();
        yield return new NotBlood();
        yield return new DosBox();
        yield return new ZHRecomp(new ConfigProviderFake());
    }

    /// <summary>
    ///     Tests that installing and uninstalling a port works correctly.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetPorts))]
    public async Task InstallPortTest(BasePort port)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        Mock<IHttpClientFactory> httpFactory = new();
        httpFactory.Setup(x => x.CreateClient(string.Empty)).Returns(() => GetHttpClient());
        httpFactory.Setup(x => x.CreateClient(HttpClientEnum.GitHub.GetDescription())).Returns(() => GetHttpClient());

        PortsRepoReleasesProvider portsRepoReleasesProvider = new(NullLogger<PortsRepoReleasesProvider>.Instance, httpFactory.Object);
        ToolsRepoReleasesProvider toolsRepoReleasesProvider = new(NullLogger<ToolsRepoReleasesProvider>.Instance, httpFactory.Object);
        AppRepoReleasesProvider appRepoReleasesProvider = new(NullLogger<AppRepoReleasesProvider>.Instance, httpFactory.Object);

        FilesDownloader filesDownloader = new(httpFactory.Object, NullLogger<FilesDownloader>.Instance);
        ArchiveTools archiveTools = new(NullLogger<ArchiveTools>.Instance);

        GitHubApiInterface gitHubApiInterface = new(
            portsRepoReleasesProvider,
            toolsRepoReleasesProvider,
            appRepoReleasesProvider,
            httpFactory.Object,
            NullLogger<GitHubApiInterface>.Instance
            );

        PortInstallerFactory portInstallerFactory = new(
            gitHubApiInterface,
            filesDownloader,
            archiveTools,
            NullLoggerFactory.Instance
            );

        var installer = portInstallerFactory.Create(port);

        var installResult = await installer.InstallAsync();

        Assert.True(installResult.IsSuccess, $"Port install failed: {installResult.Message}");

        Assert.True(File.Exists(Path.Combine(port.InstallFolderPath, "version")), "Version file does not exist.");
        Assert.True(File.Exists(port.PortExeFilePath), "Port exe does not exist.");

        installer.Uninstall();

        Assert.False(File.Exists(Path.Combine(port.InstallFolderPath, "version")), "Version file still exists.");
        Assert.False(File.Exists(port.PortExeFilePath), "Port exe still exists.");
    }


    private static HttpClient GetHttpClient()
    {
        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
        httpClient.Timeout = TimeSpan.FromSeconds(39);

        return httpClient;
    }
}
