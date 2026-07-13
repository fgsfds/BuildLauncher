using Core.Client.Interfaces;
using Core.Client.Tools;
using Games.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Tools.Installer;
using Tools.Tools;

namespace Tests.Unit;

public sealed class ToolInstallerFactoryTests
{
    private readonly ToolInstallerFactory _factory;

    public ToolInstallerFactoryTests()
    {
        var apiMock = new Mock<IApiInterface>();
        var gamesProviderMock = new Mock<InstalledGamesProvider>();
        var downloader = new FilesDownloader(new Mock<IHttpClientFactory>().Object, NullLogger<FilesDownloader>.Instance);
        var archiveTools = new ArchiveTools(NullLogger<ArchiveTools>.Instance);
        _factory = new ToolInstallerFactory(apiMock.Object, gamesProviderMock.Object, downloader, archiveTools, NullLoggerFactory.Instance);
    }

    [Fact]
    public void Create_WithDOSBlood_ReturnsToolInstaller()
    {
        var gamesProviderMock = new Mock<InstalledGamesProvider>();
        var installer = _factory.Create(new DOSBlood(gamesProviderMock.Object));
        Assert.NotNull(installer);
    }

    [Fact]
    public void Create_WithMapster32_ReturnsToolInstaller()
    {
        var gamesProviderMock = new Mock<InstalledGamesProvider>();
        var installer = _factory.Create(new Mapster32(gamesProviderMock.Object));
        Assert.NotNull(installer);
    }

    [Fact]
    public void Create_WithXMapEdit_ReturnsToolInstaller()
    {
        var gamesProviderMock = new Mock<InstalledGamesProvider>();
        var installer = _factory.Create(new XMapEdit(gamesProviderMock.Object));
        Assert.NotNull(installer);
    }
}
