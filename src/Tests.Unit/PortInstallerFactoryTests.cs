using Core.Client.Interfaces;
using Core.Client.Tools;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Ports.Installer;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Tests.Unit;

public sealed class PortInstallerFactoryTests
{
    private readonly PortInstallerFactory _factory;

    public PortInstallerFactoryTests()
    {
        var apiMock = new Mock<IApiInterface>();
        var downloader = new FilesDownloader(new Mock<IHttpClientFactory>().Object, NullLogger<FilesDownloader>.Instance);
        var archiveTools = new ArchiveTools(NullLogger<ArchiveTools>.Instance);
        _factory = new PortInstallerFactory(apiMock.Object, downloader, archiveTools, NullLoggerFactory.Instance);
    }

    [Fact]
    public void Create_WithStubPort_ReturnsPortInstaller()
    {
        var installer = _factory.Create(new StubPort());
        Assert.NotNull(installer);
    }

    [Fact]
    public void Create_WithDosBox_ReturnsPortInstaller()
    {
        var installer = _factory.Create(new DosBox());
        Assert.NotNull(installer);
    }

    [Fact]
    public void Create_WithEDuke32_ReturnsPortInstaller()
    {
        var installer = _factory.Create(new EDuke32());
        Assert.NotNull(installer);
    }

    [Fact]
    public void Create_WithRaze_ReturnsPortInstaller()
    {
        var installer = _factory.Create(new Raze());
        Assert.NotNull(installer);
    }
}
