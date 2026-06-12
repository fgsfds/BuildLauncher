using Core.All.Enums;
using Core.All.Helpers;
using Core.All.Providers;
using Core.Client.Api;
using Core.Client.Tools;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Ports.Installer;
using Ports.Ports;
using Ports.Ports.EDuke32;
using Ports.Providers;
using Tools.Providers;

namespace Tests.Unit;

public sealed class PortsInstallerTests
{
    public static IEnumerable<TheoryDataRow<BasePort>> GetPorts()
    {
        yield return new EDuke32();
        yield return new Raze();
        yield return new NBlood();
        yield return new NotBlood();
        yield return new DosBox();
    }

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

        PortsReleasesProvider portsReleasesProvider = new(NullLogger<PortsReleasesProvider>.Instance, httpFactory.Object);
        ToolsReleasesProvider toolsReleasesProvider = new(NullLogger<ToolsReleasesProvider>.Instance, httpFactory.Object);
        RepoAppReleasesProvider repoAppReleasesProvider = new(NullLogger<RepoAppReleasesProvider>.Instance, httpFactory.Object);

        FilesDownloader filesDownloader = new(httpFactory.Object, NullLogger<FilesDownloader>.Instance);
        ArchiveTools archiveTools = new(NullLogger<ArchiveTools>.Instance);

        GitHubApiInterface gitHubApiInterface = new(
            portsReleasesProvider,
            toolsReleasesProvider,
            repoAppReleasesProvider,
            httpFactory.Object,
            NullLogger<GitHubApiInterface>.Instance
            );

        PortInstallerFactory portInstallerFactory = new(
            gitHubApiInterface,
            filesDownloader,
            archiveTools,
            NullLoggerFactory.Instance
            );

        //Raze port = new();
        var installer = portInstallerFactory.Create(port);

        await installer.InstallAsync();

        Assert.True(File.Exists(Path.Combine(port.InstallFolderPath, "version")));
        Assert.True(File.Exists(port.PortExeFilePath));

        installer.Uninstall();

        Assert.False(File.Exists(Path.Combine(port.InstallFolderPath, "version")));
        Assert.False(File.Exists(port.PortExeFilePath));
    }


    HttpClient GetHttpClient()
    {
        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
        httpClient.Timeout = TimeSpan.FromSeconds(39);
        return httpClient;
    }
}
