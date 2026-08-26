using Core.All.Enums;
using Core.All.Helpers;
using Core.Client.Api;
using Core.Client.Config;
using Core.Client.Releases;
using Core.Client.Tools;
using Games.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Ports.Releases;
using Tools.Installer;
using Tools.Releases;
using Tools.Tools;

namespace Tests.External;

/// <summary>
///     Tests for tool installation.
/// </summary>
public sealed class ToolsInstallerTests
{
    /// <summary>
    ///     Gets theory data with all tools to test installation.
    /// </summary>
    public static IEnumerable<TheoryDataRow<ToolEnum>> GetTools()
    {
        yield return ToolEnum.XMapEdit;
        yield return ToolEnum.DOSBlood;
    }

    /// <summary>
    ///     Tests that installing and uninstalling a tool works correctly.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetTools))]
    public async Task InstallToolTest(ToolEnum toolEnum)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var bloodDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(bloodDir);
        File.WriteAllText(Path.Combine(bloodDir, "BLOOD.EXE"), "original");
        File.WriteAllText(Path.Combine(bloodDir, "BLOOD.INI"), "ini");
        File.WriteAllText(Path.Combine(bloodDir, "game.art"), "art");

        try
        {
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

            var fakeConfig = new ConfigProviderFake();
            fakeConfig.SetGamePath(GameEnum.Blood, bloodDir);

            InstalledGamesProvider gamesProvider = new(fakeConfig);

            BaseTool tool = toolEnum switch
            {
                ToolEnum.XMapEdit => new XMapEdit(gamesProvider),
                ToolEnum.DOSBlood => new DOSBlood(gamesProvider),
                _ => throw new ArgumentOutOfRangeException(nameof(toolEnum), toolEnum, null)
            };

            ToolInstallerFactory toolInstallerFactory = new(
                gitHubApiInterface,
                gamesProvider,
                filesDownloader,
                archiveTools,
                NullLoggerFactory.Instance
                );

            var installer = toolInstallerFactory.Create(tool);

            var installResult = await installer.InstallAsync();

            Assert.True(installResult.IsSuccess, $"Tool install failed: {installResult.Message}");

            Assert.True(File.Exists(Path.Combine(tool.InstallFolderPath, "version")), "Version file does not exist.");
            Assert.True(File.Exists(tool.ToolExeFilePath), "Tool exe does not exist.");

            installer.Uninstall();

            Assert.False(File.Exists(Path.Combine(tool.InstallFolderPath, "version")), "Version file still exists.");

            if (tool.ToolEnum is ToolEnum.DOSBlood)
            {
                Assert.True(File.Exists(tool.ToolExeFilePath), "Original BLOOD.EXE should be restored on uninstall.");
            }
            else
            {
                Assert.False(File.Exists(tool.ToolExeFilePath), "Tool exe still exists.");
            }
        }
        finally
        {
            if (Directory.Exists(bloodDir))
            {
                Directory.Delete(bloodDir, true);
            }
        }
    }

    private static HttpClient GetHttpClient()
    {
        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
        httpClient.Timeout = TimeSpan.FromSeconds(39);

        return httpClient;
    }
}
