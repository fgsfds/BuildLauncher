using Common.All.Enums;
using Common.All.Helpers;
using Common.All.Providers;
using Common.Client.Api;
using Common.Client.Tools;
using Microsoft.Extensions.Logging;
using Moq;
using Ports.Installer;
using Ports.Ports;
using Ports.Ports.EDuke32;
using Ports.Providers;
using Tools.Providers;

namespace Tests;

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

        Mock<ILogger> logger = new();
        Mock<IHttpClientFactory> httpFactory = new();
        httpFactory.Setup(x => x.CreateClient(string.Empty)).Returns(() => GetHttpClient());
        httpFactory.Setup(x => x.CreateClient(HttpClientEnum.GitHub.GetDescription())).Returns(() => GetHttpClient());

        PortsReleasesProvider portsReleasesProvider = new(logger.Object, httpFactory.Object);
        ToolsReleasesProvider toolsReleasesProvider = new(logger.Object, httpFactory.Object);
        RepoAppReleasesProvider repoAppReleasesProvider = new(logger.Object, httpFactory.Object);

        FilesDownloader filesDownloader = new(httpFactory.Object, logger.Object);
        ArchiveTools archiveTools = new(logger.Object);

        GitHubApiInterface gitHubApiInterface = new(
            portsReleasesProvider,
            toolsReleasesProvider,
            repoAppReleasesProvider,
            httpFactory.Object,
            logger.Object
            );

        PortInstallerFactory portInstallerFactory = new(
            gitHubApiInterface,
            filesDownloader,
            archiveTools,
            logger.Object
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
