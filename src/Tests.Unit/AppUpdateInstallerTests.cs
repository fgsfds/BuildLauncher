using System.Security.Cryptography;
using Core.Client;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Core.Client.Tools;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Tests.Unit.Helpers;

namespace Tests.Unit;

/// <summary> Verifies self-update preparation and rollback without restarting the test process. </summary>
public sealed class AppUpdateInstallerTests : IDisposable
{
    /// <summary> The isolated application folder. </summary>
    private readonly string _root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    /// <summary> Creates an existing executable. </summary>
    public AppUpdateInstallerTests()
    {
        _ = Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, "app.exe"), "old");
    }

    /// <summary> Removes only test-owned files. </summary>
    public void Dispose() => Directory.Delete(_root, true);

    /// <summary> Deferred cleanup removes completed backups but never incomplete recovery data. </summary>
    [Fact]
    public void CleanupCompletedUpdates_PreservesRecoveryBackups()
    {
        var completed = Path.Combine(_root, ".update-backup-" + Guid.NewGuid().ToString("N"));
        var recovery = Path.Combine(_root, ".update-backup-" + Guid.NewGuid().ToString("N"));
        var unrelated = Path.Combine(_root, ".update-backup-user");
        foreach (var path in new[] { completed, recovery, unrelated })
        {
            _ = Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, "app.exe"), "original");
        }

        File.WriteAllText(completed + ".cleanup-ready", string.Empty);
        File.WriteAllText(unrelated + ".cleanup-ready", string.Empty);
        AppUpdateInstaller.CleanupCompletedUpdates(_root);
        AppUpdateInstaller.CleanupCompletedUpdates(_root);
        Assert.False(Directory.Exists(completed));
        Assert.Equal("original", File.ReadAllText(Path.Combine(recovery, "app.exe")));
        Assert.Equal("original", File.ReadAllText(Path.Combine(unrelated, "app.exe")));
    }

    /// <summary> A locked completed backup remains eligible for cleanup after the handle closes. </summary>
    [Fact]
    public void CleanupCompletedUpdates_LockedFile_RetriesOnNextStartup()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var backup = Path.Combine(_root, ".update-backup-" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(backup);
        File.WriteAllText(backup + ".cleanup-ready", string.Empty);
        var executable = Path.Combine(backup, "app.exe");
        using (var locked = new FileStream(executable, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
        {
            AppUpdateInstaller.CleanupCompletedUpdates(_root);
            Assert.True(File.Exists(executable));
            Assert.True(File.Exists(backup + ".cleanup-ready"));
        }

        AppUpdateInstaller.CleanupCompletedUpdates(_root);
        Assert.False(Directory.Exists(backup));
    }

    /// <summary> Invalid or canceled updates do not publish an update marker. </summary>
    /// <param name="failure">The invalid update condition.</param>
    [Theory]
    [InlineData("hash")]
    [InlineData("missing")]
    [InlineData("corrupt")]
    [InlineData("cancel")]
    [InlineData("http")]
    public async Task Prepare_Failure_PreservesCurrentExecutable(string failure)
    {
        var data = failure == "corrupt" ? new byte[] { 1, 2, 3 } : ReleaseTestHelper.Zip((failure == "missing" ? "other.txt" : "app.exe", "new"));
        var api = new Mock<IApiInterface>();
        api.Setup(x => x.GetLatestAppReleaseAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ReleaseTestHelper.Release(failure == "hash" ? "bad" : null));
        var downloader = new FilesDownloader(ReleaseTestHelper.Http(data, failure == "http" ? System.Net.HttpStatusCode.ServiceUnavailable : System.Net.HttpStatusCode.OK), NullLogger<FilesDownloader>.Instance);
        var installer = new AppUpdateInstaller(downloader, api.Object, NullLogger<AppUpdateInstaller>.Instance);
        Assert.True(await installer.CheckForUpdates(new Version(1, 0), TestContext.Current.CancellationToken));
        using var cancellation = new CancellationTokenSource();
        if (failure == "cancel")
        {
            cancellation.Cancel();
        }

        _ = await Assert.ThrowsAnyAsync<Exception>(() => installer.DownloadAndUnpackLatestRelease(_root, "app.exe", cancellation.Token));
        Assert.Equal("old", File.ReadAllText(Path.Combine(_root, "app.exe")));
        Assert.False(File.Exists(Path.Combine(_root, ClientConsts.UpdateFile)));
        Assert.False(Directory.Exists(Path.Combine(_root, ClientConsts.UpdateFolder)));
        Assert.Empty(Directory.GetDirectories(_root));
    }

    /// <summary> Fresh preparation removes stale payloads and installs companion files along with the executable. </summary>
    [Fact]
    public async Task PrepareAndApply_Success_ReplacesCompletePayload()
    {
        var update = Path.Combine(_root, ClientConsts.UpdateFolder);
        _ = Directory.CreateDirectory(update);
        File.WriteAllText(Path.Combine(update, "stale.dll"), "stale");
        File.WriteAllText(Path.Combine(_root, "user.cfg"), "keep");
        var data = ReleaseTestHelper.Zip(("app.exe", "new"), ("lib/runtime.dll", "library"));
        var api = new Mock<IApiInterface>();
        api.Setup(x => x.GetLatestAppReleaseAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ReleaseTestHelper.Release("sha256:" + Convert.ToHexString(SHA256.HashData(data))));
        var installer = new AppUpdateInstaller(new FilesDownloader(ReleaseTestHelper.Http(data), NullLogger<FilesDownloader>.Instance), api.Object, NullLogger<AppUpdateInstaller>.Instance);
        Assert.True(await installer.CheckForUpdates(new Version(1, 0), TestContext.Current.CancellationToken));
        await installer.DownloadAndUnpackLatestRelease(_root, "app.exe", TestContext.Current.CancellationToken);
        Assert.False(File.Exists(Path.Combine(update, "stale.dll")));
        Assert.True(File.Exists(Path.Combine(_root, ClientConsts.UpdateFile)));

        AppUpdateInstaller.ApplyPreparedUpdate(_root, "app.exe");

        Assert.Equal("new", File.ReadAllText(Path.Combine(_root, "app.exe")));
        Assert.Equal("library", File.ReadAllText(Path.Combine(_root, "lib", "runtime.dll")));
        Assert.Equal("keep", File.ReadAllText(Path.Combine(_root, "user.cfg")));
        Assert.False(File.Exists(Path.Combine(_root, ClientConsts.UpdateFile)));
    }

    /// <summary> A missing executable never causes the live executable to be renamed. </summary>
    [Fact]
    public void Apply_MissingExecutable_PreservesCurrentExecutable()
    {
        _ = Directory.CreateDirectory(Path.Combine(_root, ClientConsts.UpdateFolder));
        Assert.Throws<InvalidDataException>(() => AppUpdateInstaller.ApplyPreparedUpdate(_root, "app.exe"));
        Assert.Equal("old", File.ReadAllText(Path.Combine(_root, "app.exe")));
    }

    /// <summary> A later replacement failure restores already-replaced files. </summary>
    [Fact]
    public void Apply_FileCollision_RollsBackExecutable()
    {
        var update = Path.Combine(_root, ClientConsts.UpdateFolder);
        _ = Directory.CreateDirectory(update);
        File.WriteAllText(Path.Combine(update, "app.exe"), "new");
        File.WriteAllText(Path.Combine(update, "z-blocked"), "payload");
        _ = Directory.CreateDirectory(Path.Combine(_root, "z-blocked"));
        File.WriteAllText(Path.Combine(_root, "z-blocked", "user.txt"), "keep");
        File.WriteAllText(Path.Combine(_root, ClientConsts.UpdateFile), string.Empty);

        Assert.ThrowsAny<IOException>(() => AppUpdateInstaller.ApplyPreparedUpdate(_root, "app.exe"));

        Assert.Equal("old", File.ReadAllText(Path.Combine(_root, "app.exe")));
        Assert.Equal("keep", File.ReadAllText(Path.Combine(_root, "z-blocked", "user.txt")));
        Assert.True(File.Exists(Path.Combine(_root, ClientConsts.UpdateFile)));
    }
}
