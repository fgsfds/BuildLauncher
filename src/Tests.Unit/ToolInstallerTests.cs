using Core.All.Enums;
using Core.Client.Interfaces;
using Core.Client.Tools;
using Games.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Tools.Installer;
using Tools.Tools;

namespace Tests.Unit;

public sealed class ToolInstallerTests
{
    [Fact]
    public void Create_Mapster32_ReturnsToolInstaller()
    {
        var apiMock = new Mock<IApiInterface>();
        var gamesMock = new Mock<InstalledGamesProvider>();
        var downloader = new FilesDownloader(new Mock<IHttpClientFactory>().Object, NullLogger<FilesDownloader>.Instance);
        var archiveTools = new ArchiveTools(NullLogger<ArchiveTools>.Instance);

        var mapster32 = new Mapster32(gamesMock.Object);
        var installer = new ToolInstaller(mapster32, apiMock.Object, gamesMock.Object, downloader, archiveTools, NullLogger<ToolInstaller>.Instance);

        Assert.NotNull(installer);
    }

    [Fact]
    public void Create_DOSBlood_ReturnsToolInstaller()
    {
        var apiMock = new Mock<IApiInterface>();
        var gamesMock = new Mock<InstalledGamesProvider>();
        var downloader = new FilesDownloader(new Mock<IHttpClientFactory>().Object, NullLogger<FilesDownloader>.Instance);
        var archiveTools = new ArchiveTools(NullLogger<ArchiveTools>.Instance);

        var dosBlood = new DOSBlood(gamesMock.Object);
        var installer = new ToolInstaller(dosBlood, apiMock.Object, gamesMock.Object, downloader, archiveTools, NullLogger<ToolInstaller>.Instance);

        Assert.NotNull(installer);
    }

    [Fact]
    public void Create_XMapEdit_ReturnsToolInstaller()
    {
        var apiMock = new Mock<IApiInterface>();
        var gamesMock = new Mock<InstalledGamesProvider>();
        var downloader = new FilesDownloader(new Mock<IHttpClientFactory>().Object, NullLogger<FilesDownloader>.Instance);
        var archiveTools = new ArchiveTools(NullLogger<ArchiveTools>.Instance);

        var xmapedit = new XMapEdit(gamesMock.Object);
        var installer = new ToolInstaller(xmapedit, apiMock.Object, gamesMock.Object, downloader, archiveTools, NullLogger<ToolInstaller>.Instance);

        Assert.NotNull(installer);
    }

    [Fact]
    public async Task GetRelease_DelegatesToApiInterface()
    {
        var apiMock = new Mock<IApiInterface>();
        apiMock.Setup(x => x.GetLatestToolReleaseAsync(It.IsAny<ToolEnum>())).ReturnsAsync((Core.All.Serializable.Downloadable.GeneralReleaseJsonModel?)null);

        var gamesMock = new Mock<InstalledGamesProvider>();
        var downloader = new FilesDownloader(new Mock<IHttpClientFactory>().Object, NullLogger<FilesDownloader>.Instance);
        var archiveTools = new ArchiveTools(NullLogger<ArchiveTools>.Instance);

        var dosBlood = new DOSBlood(gamesMock.Object);
        var installer = new ToolInstaller(dosBlood, apiMock.Object, gamesMock.Object, downloader, archiveTools, NullLogger<ToolInstaller>.Instance);

        var release = await installer.GetRelease();

        Assert.Null(release);
        apiMock.Verify(x => x.GetLatestToolReleaseAsync(ToolEnum.DOSBlood), Times.Once);
    }
}
