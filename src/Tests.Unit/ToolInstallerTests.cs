using System.Reflection;
using Core.All.Enums;
using Core.All.Serializable.Downloadable;
using Core.Client.Config;
using Core.Client.Helpers;
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
        var gamesMock = new Mock<InstalledGamesProvider>(Mock.Of<IConfigProvider>());
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
        var gamesMock = new Mock<InstalledGamesProvider>(Mock.Of<IConfigProvider>());
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
        var gamesMock = new Mock<InstalledGamesProvider>(Mock.Of<IConfigProvider>());
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
        apiMock.Setup(x => x.GetLatestToolReleaseAsync(It.IsAny<ToolEnum>())).ReturnsAsync((GeneralReleaseJsonModel?)null);

        var gamesMock = new Mock<InstalledGamesProvider>(Mock.Of<IConfigProvider>());
        var downloader = new FilesDownloader(new Mock<IHttpClientFactory>().Object, NullLogger<FilesDownloader>.Instance);
        var archiveTools = new ArchiveTools(NullLogger<ArchiveTools>.Instance);

        var dosBlood = new DOSBlood(gamesMock.Object);
        var installer = new ToolInstaller(dosBlood, apiMock.Object, gamesMock.Object, downloader, archiveTools, NullLogger<ToolInstaller>.Instance);

        var release = await installer.GetRelease();

        Assert.Null(release);
        apiMock.Verify(x => x.GetLatestToolReleaseAsync(ToolEnum.DOSBlood), Times.Once);
    }

    private static ToolInstaller CreateInstaller(BaseTool tool, InstalledGamesProvider gamesProvider)
    {
        var apiMock = new Mock<IApiInterface>();
        var downloader = new FilesDownloader(new Mock<IHttpClientFactory>().Object, NullLogger<FilesDownloader>.Instance);
        var archiveTools = new ArchiveTools(NullLogger<ArchiveTools>.Instance);

        return new ToolInstaller(tool, apiMock.Object, gamesProvider, downloader, archiveTools, NullLogger<ToolInstaller>.Instance);
    }

    private static InstalledGamesProvider CreateGamesProvider(string? bloodPath) =>
        new(new ConfigProviderFake { PathBlood = bloodPath });

    private static void InvokeProtected(ToolInstaller installer, string methodName) =>
        typeof(ToolInstaller).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(installer, null);

    [Fact]
    public void DOSBlood_Backup_RenamesBloodExeToBak()
    {
        var bloodDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(bloodDir);
        var bloodExe = Path.Combine(bloodDir, "BLOOD.EXE");
        File.WriteAllText(bloodExe, "blood");

        try
        {
            var games = CreateGamesProvider(bloodDir);
            var installer = CreateInstaller(new DOSBlood(games), games);

            InvokeProtected(installer, "Backup");

            Assert.False(File.Exists(bloodExe), "Original BLOOD.EXE should be moved.");
            Assert.True(File.Exists(bloodExe + ".BAK"), "Backup BLOOD.EXE.BAK should exist.");
        }
        finally
        {
            Directory.Delete(bloodDir, true);
        }
    }

    [Fact]
    public void DOSBlood_Backup_ExistingBak_DoesNotOverwrite()
    {
        var bloodDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(bloodDir);
        var bloodExe = Path.Combine(bloodDir, "BLOOD.EXE");
        var bloodExeBak = bloodExe + ".BAK";
        File.WriteAllText(bloodExe, "blood");
        File.WriteAllText(bloodExeBak, "existing bak");

        try
        {
            var games = CreateGamesProvider(bloodDir);
            var installer = CreateInstaller(new DOSBlood(games), games);

            InvokeProtected(installer, "Backup");

            Assert.True(File.Exists(bloodExe), "Original BLOOD.EXE should remain when a backup already exists.");
            Assert.Equal("existing bak", File.ReadAllText(bloodExeBak));
        }
        finally
        {
            Directory.Delete(bloodDir, true);
        }
    }

    [Fact]
    public void DOSBlood_Uninstall_DeletesBloodExeAndVersionAndRestoresBak()
    {
        var bloodDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(bloodDir);
        var bloodExe = Path.Combine(bloodDir, "BLOOD.EXE");
        var bloodExeBak = bloodExe + ".BAK";
        var versionFile = Path.Combine(bloodDir, "version");
        File.WriteAllText(bloodExe, "blood");
        File.WriteAllText(bloodExeBak, "original exe");
        File.WriteAllText(versionFile, "1.0");

        try
        {
            var games = CreateGamesProvider(bloodDir);
            var installer = CreateInstaller(new DOSBlood(games), games);

            installer.Uninstall();

            Assert.Equal("original exe", File.ReadAllText(bloodExe));
            Assert.False(File.Exists(bloodExeBak), "BLOOD.EXE.BAK should be removed after restore.");
            Assert.False(File.Exists(versionFile), "version file should be deleted.");
        }
        finally
        {
            Directory.Delete(bloodDir, true);
        }
    }

    [Fact]
    public void DOSBlood_Uninstall_NullInstallFolder_Throws()
    {
        var games = CreateGamesProvider(null);
        var installer = CreateInstaller(new DOSBlood(games), games);

        Assert.Throws<InvalidOperationException>(() => installer.Uninstall());
    }

    [Fact]
    public void XMapEdit_PostInstall_CopiesFilesSkippingExeOggTxtVersion()
    {
        var bloodDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(bloodDir);
        File.WriteAllText(Path.Combine(bloodDir, "BLOOD.EXE"), "skip");
        File.WriteAllText(Path.Combine(bloodDir, "sound.ogg"), "skip");
        File.WriteAllText(Path.Combine(bloodDir, "readme.txt"), "skip");
        File.WriteAllText(Path.Combine(bloodDir, "version"), "skip");
        File.WriteAllText(Path.Combine(bloodDir, "game.art"), "copy");
        File.WriteAllText(Path.Combine(bloodDir, "game.map"), "copy");

        var installFolder = Path.Combine(ClientProperties.ToolsFolderPath, new XMapEdit(null!).Name);

        try
        {
            Directory.CreateDirectory(installFolder);
            var games = CreateGamesProvider(bloodDir);
            var installer = CreateInstaller(new XMapEdit(games), games);

            InvokeProtected(installer, "PostInstall");

            Assert.True(File.Exists(Path.Combine(installFolder, "game.art")), "game.art should be copied.");
            Assert.True(File.Exists(Path.Combine(installFolder, "game.map")), "game.map should be copied.");
            Assert.False(File.Exists(Path.Combine(installFolder, "BLOOD.EXE")), "BLOOD.EXE should be skipped.");
            Assert.False(File.Exists(Path.Combine(installFolder, "sound.ogg")), "sound.ogg should be skipped.");
            Assert.False(File.Exists(Path.Combine(installFolder, "readme.txt")), "readme.txt should be skipped.");
            Assert.False(File.Exists(Path.Combine(installFolder, "version")), "version should be skipped.");
        }
        finally
        {
            if (Directory.Exists(installFolder))
            {
                Directory.Delete(installFolder, true);
            }

            Directory.Delete(bloodDir, true);
        }
    }

    [Fact]
    public void XMapEdit_Uninstall_DeletesInstallFolder()
    {
        var installFolder = Path.Combine(ClientProperties.ToolsFolderPath, new XMapEdit(null!).Name);
        Directory.CreateDirectory(installFolder);
        File.WriteAllText(Path.Combine(installFolder, "game.art"), "data");

        try
        {
            var games = CreateGamesProvider(null);
            var installer = CreateInstaller(new XMapEdit(games), games);

            installer.Uninstall();

            Assert.False(Directory.Exists(installFolder), "XMapEdit install folder should be deleted.");
        }
        finally
        {
            if (Directory.Exists(installFolder))
            {
                Directory.Delete(installFolder, true);
            }
        }
    }
}
