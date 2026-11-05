using System.Net;
using Core.All.Enums;
using Core.Client.Config;
using Core.Client.Interfaces;
using Core.Client.Tools;
using Games.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Tests.Unit.Helpers;
using Tools.Installer;
using Tools.Tools;

namespace Tests.Unit;

/// <summary> Verifies tool installation through its public API in isolated directories. </summary>
public sealed class ToolInstallerTests : IDisposable
{
    /// <summary> The test-owned root. </summary>
    private readonly string _root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    /// <summary> Creates a fake Blood installation. </summary>
    public ToolInstallerTests()
    {
        _ = Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, "BLOOD.EXE"), "original");
        File.WriteAllText(Path.Combine(_root, "game.art"), "art");
    }

    /// <summary> Deletes only test-owned files. </summary>
    public void Dispose() => Directory.Delete(_root, true);

    /// <summary> Creates the game's configuration without accessing a live installation. </summary>
    /// <returns>The installed game provider.</returns>
    private InstalledGamesProvider Games()
    {
        var config = new ConfigProviderFake();
        config.SetGamePath(GameEnum.Blood, _root);
        return new InstalledGamesProvider(config);
    }

    /// <summary> Creates an installer with an in-memory release download. </summary>
    /// <param name="tool">The tool being installed.</param>
    /// <param name="games">The game paths.</param>
    /// <param name="data">The downloaded archive.</param>
    /// <param name="status">The fake HTTP response status.</param>
    /// <returns>The installer.</returns>
    private static ToolInstaller Installer(BaseTool tool, InstalledGamesProvider games, byte[] data, HttpStatusCode status = HttpStatusCode.OK)
    {
        var api = new Mock<IApiInterface>();
        api.Setup(x => x.GetLatestToolReleaseAsync(It.IsAny<ToolEnum>(), It.IsAny<CancellationToken>())).ReturnsAsync(ReleaseTestHelper.Release());
        return new ToolInstaller(tool, api.Object, games,
            new FilesDownloader(ReleaseTestHelper.Http(data, status), NullLogger<FilesDownloader>.Instance),
            new ArchiveTools(NullLogger<ArchiveTools>.Instance), NullLogger<ToolInstaller>.Instance);
    }

    /// <summary> DOSBlood updates retain the original backup and uninstall restores it. </summary>
    /// <param name="existingBackup">Whether a previous install already made a backup.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DOSBlood_InstallAndUninstall_PreservesOriginal(bool existingBackup)
    {
        var exe = Path.Combine(_root, "BLOOD.EXE");
        if (existingBackup)
        {
            File.WriteAllText(exe + ".BAK", "backup");
        }

        var games = Games();
        var installer = Installer(new DOSBlood(games), games, ReleaseTestHelper.Zip(("BLOOD.EXE", "updated")));
        var result = await installer.InstallAsync(TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal("updated", File.ReadAllText(exe));
        Assert.Equal(existingBackup ? "backup" : "original", File.ReadAllText(exe + ".BAK"));
        Assert.Equal("art", File.ReadAllText(Path.Combine(_root, "game.art")));

        installer.Uninstall();
        Assert.Equal(existingBackup ? "backup" : "original", File.ReadAllText(exe));
        Assert.False(File.Exists(exe + ".BAK"));
    }

    /// <summary> Failure before or after backup preparation leaves the original executable intact. </summary>
    /// <param name="networkFailure">Whether HTTP fails before extraction.</param>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DOSBlood_FailedInstall_DoesNotDeleteExecutable(bool networkFailure)
    {
        var games = Games();
        var installer = Installer(new DOSBlood(games), games, [1, 2, 3], networkFailure ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK);
        var result = await installer.InstallAsync(TestContext.Current.CancellationToken);
        Assert.False(result.IsSuccess);
        Assert.Equal("original", File.ReadAllText(Path.Combine(_root, "BLOOD.EXE")));
        Assert.False(File.Exists(Path.Combine(_root, "BLOOD.EXE.BAK")));
    }

    /// <summary> XMapEdit copies game resources while excluding unrelated executables and music. </summary>
    [Fact]
    public async Task XMapEdit_Install_CopiesGameResources()
    {
        File.WriteAllText(Path.Combine(_root, "track.ogg"), "music");
        var destination = Path.Combine(_root, "editor");
        var tool = new Mock<BaseTool>();
        tool.SetupGet(x => x.InstallFolderPath).Returns(destination);
        tool.SetupGet(x => x.ToolEnum).Returns(ToolEnum.XMapEdit);
        var installer = Installer(tool.Object, Games(), ReleaseTestHelper.Zip(("editor.exe", "editor")));
        var result = await installer.InstallAsync(TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal("art", File.ReadAllText(Path.Combine(destination, "game.art")));
        Assert.False(File.Exists(Path.Combine(destination, "BLOOD.EXE")));
        Assert.False(File.Exists(Path.Combine(destination, "track.ogg")));
        Assert.Equal("2.0", File.ReadAllText(Path.Combine(destination, "version")));

        installer.Uninstall();
        Assert.False(Directory.Exists(destination));
        Assert.Equal("original", File.ReadAllText(Path.Combine(_root, "BLOOD.EXE")));
    }
}
