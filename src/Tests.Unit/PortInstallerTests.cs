using Core.All.Enums;
using Core.Client.Interfaces;
using Core.Client.Tools;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Tests.Unit.Helpers;
using Ports.Installer;
using Ports.Ports;

namespace Tests.Unit;

public sealed class PortInstallerTests
{
    /// <summary> Extracted Linux ports can execute even when ZIP entries have no executable permission. </summary>
    [Fact]
    public async Task InstallAsync_LinuxPort_SetsExecutePermission()
    {
        if (!OperatingSystem.IsLinux()) { return; }
        var root = Path.Combine(Path.GetTempPath(), "port with spaces " + Guid.NewGuid().ToString("N"));
        try
        {
            var port = new Mock<BasePort>();
            port.SetupGet(x => x.InstallFolderPath).Returns(root);
            port.SetupGet(x => x.PortEnum).Returns(PortEnum.Raze);
            port.Protected().SetupGet<string>("LinExe").Returns("runner");
            var api = new Mock<IApiInterface>();
            api.Setup(x => x.GetLatestPortReleaseAsync(PortEnum.Raze, It.IsAny<CancellationToken>())).ReturnsAsync(ReleaseTestHelper.Release());
            var script = string.Join(Environment.NewLine, "#!/bin/sh", "exit 0", "");
            var downloader = new FilesDownloader(ReleaseTestHelper.Http(ReleaseTestHelper.Zip(("runner", script))), NullLogger<FilesDownloader>.Instance);
            var installer = new PortInstaller(port.Object, api.Object, downloader,
                new ArchiveTools(NullLogger<ArchiveTools>.Instance), NullLogger<PortInstaller>.Instance);
            var result = await installer.InstallAsync(TestContext.Current.CancellationToken);
            Assert.True(result.IsSuccess, result.Message);
            var executable = Path.Combine(root, "runner");
            Assert.NotEqual((UnixFileMode)0, File.GetUnixFileMode(executable) & UnixFileMode.UserExecute);
            using var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(executable) { UseShellExecute = false });
            Assert.NotNull(process);
            await process.WaitForExitAsync(TestContext.Current.CancellationToken);
            Assert.Equal(0, process.ExitCode);
        }
        finally { if (Directory.Exists(root)) { Directory.Delete(root, true); } }
    }

    [Fact]
    public void Create_StubPort_ReturnsPortInstaller()
    {
        var apiMock = new Mock<IApiInterface>();
        var downloader = new FilesDownloader(new Mock<IHttpClientFactory>().Object, NullLogger<FilesDownloader>.Instance);
        var archiveTools = new ArchiveTools(NullLogger<ArchiveTools>.Instance);

        var installer = new PortInstaller(new StubPort(), apiMock.Object, downloader, archiveTools, NullLogger<PortInstaller>.Instance);

        Assert.NotNull(installer);
    }

    [Fact]
    public async Task GetRelease_DelegatesToApiInterface()
    {
        var apiMock = new Mock<IApiInterface>();
        apiMock.Setup(x => x.GetLatestPortReleaseAsync(It.IsAny<PortEnum>())).ReturnsAsync((Core.All.Serializable.Downloadable.GeneralReleaseJsonModel?)null);

        var downloader = new FilesDownloader(new Mock<IHttpClientFactory>().Object, NullLogger<FilesDownloader>.Instance);
        var archiveTools = new ArchiveTools(NullLogger<ArchiveTools>.Instance);

        var installer = new PortInstaller(new StubPort(), apiMock.Object, downloader, archiveTools, NullLogger<PortInstaller>.Instance);

        var release = await installer.GetRelease();

        Assert.Null(release);
        apiMock.Verify(x => x.GetLatestPortReleaseAsync(PortEnum.Stub), Times.Once);
    }

    [Fact]
    public void Uninstall_DeletesInstallFolder()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, "version"), "1.0");

        try
        {
            var apiMock = new Mock<IApiInterface>();
            var downloader = new FilesDownloader(new Mock<IHttpClientFactory>().Object, NullLogger<FilesDownloader>.Instance);
            var archiveTools = new ArchiveTools(NullLogger<ArchiveTools>.Instance);

            var port = new Mock<BasePort>();
            port.SetupGet(x => x.InstallFolderPath).Returns(tempDir);
            var installer = new PortInstaller(port.Object, apiMock.Object, downloader, archiveTools, NullLogger<PortInstaller>.Instance);

            installer.Uninstall();

            Assert.False(Directory.Exists(tempDir));
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }
}
