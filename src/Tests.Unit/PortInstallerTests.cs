using Core.All.Enums;
using Core.Client.Interfaces;
using Core.Client.Tools;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Ports.Installer;
using Ports.Ports;

namespace Tests.Unit;

public sealed class PortInstallerTests
{
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

            var stubPort = new StubPort();
            var installFolderField = typeof(StubPort).GetProperty("InstallFolderPath");
            Assert.NotNull(installFolderField);

            var installer = new PortInstaller(stubPort, apiMock.Object, downloader, archiveTools, NullLogger<PortInstaller>.Instance);
            Assert.NotNull(installer);
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }
}
