using System.Net;
using Core.All;
using Core.All.Serializable.Downloadable;
using Core.Client;
using Core.Client.Interfaces;
using Core.Client.Tools;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Tests.Unit.Helpers;

namespace Tests.Unit;

/// <summary> Verifies failed installs preserve live files and successful installs preserve user content. </summary>
public sealed class InstallerBaseTests : IDisposable
{
    /// <summary> The isolated installation parent. </summary>
    private readonly string _root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    /// <summary> Creates an existing installation with user content. </summary>
    public InstallerBaseTests()
    {
        _ = Directory.CreateDirectory(Path.Combine(_root, "live"));
        File.WriteAllText(Path.Combine(_root, "live", "game.exe"), "old");
        File.WriteAllText(Path.Combine(_root, "live", "user.cfg"), "settings");
        File.WriteAllText(Path.Combine(_root, "live", "version"), "1.0");
    }

    /// <summary> Removes only this test's directory. </summary>
    public void Dispose() => Directory.Delete(_root, true);

    /// <summary> Every pre-commit failure preserves the complete original installation. </summary>
    /// <param name="failure">The failure to inject.</param>
    [Theory]
    [InlineData("lookup")]
    [InlineData("http")]
    [InlineData("hash")]
    [InlineData("archive")]
    [InlineData("post")]
    [InlineData("cancel")]
    [InlineData("cancel-post")]
    public async Task InstallAsync_Failure_PreservesInstallation(string failure)
    {
        var bytes = failure == "archive" ? new byte[] { 1, 2, 3 } : ReleaseTestHelper.Zip(("game.exe", "new"));
        var http = ReleaseTestHelper.Http(bytes, failure == "http" ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK);
        var instance = new Mock<IInstallable>();
        instance.SetupGet(x => x.InstallFolderPath).Returns(Path.Combine(_root, "live"));
        var installer = new TestInstaller(instance.Object, http, ReleaseTestHelper.Release(failure == "hash" ? "bad" : null));
        installer.LookupFailure = failure == "lookup";
        using var cancellation = new CancellationTokenSource();
        if (failure == "cancel")
        {
            cancellation.Cancel();
        }

        installer.AfterExtraction = () =>
        {
            if (failure == "post")
            {
                throw new IOException("Post-install failed");
            }

            if (failure == "cancel-post")
            {
                cancellation.Cancel();
            }
        };
        var result = await installer.InstallAsync(cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal(failure.StartsWith("cancel", StringComparison.Ordinal) ? ResultEnum.Cancelled : failure == "hash" ? ResultEnum.HashError : ResultEnum.Error, result.ResultEnum);
        Assert.Equal("old", File.ReadAllText(Path.Combine(_root, "live", "game.exe")));
        Assert.Equal("settings", File.ReadAllText(Path.Combine(_root, "live", "user.cfg")));
        Assert.Equal("1.0", File.ReadAllText(Path.Combine(_root, "live", "version")));
        Assert.Single(Directory.GetDirectories(_root));
    }

    /// <summary> A successful update replaces binaries while retaining user files. </summary>
    [Fact]
    public async Task InstallAsync_Success_PreservesUserFiles()
    {
        var instance = new Mock<IInstallable>();
        instance.SetupGet(x => x.InstallFolderPath).Returns(Path.Combine(_root, "live"));
        var installer = new TestInstaller(instance.Object, ReleaseTestHelper.Http(ReleaseTestHelper.Zip(("game.exe", "new"))), ReleaseTestHelper.Release());
        var result = await installer.InstallAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal("new", File.ReadAllText(Path.Combine(_root, "live", "game.exe")));
        Assert.Equal("settings", File.ReadAllText(Path.Combine(_root, "live", "user.cfg")));
        Assert.Equal("2.0", File.ReadAllText(Path.Combine(_root, "live", "version")));
    }

    /// <summary> Exposes installer failure points without changing production services. </summary>
    private sealed class TestInstaller : InstallerBase<IInstallable>
    {
        /// <summary> The fake release. </summary>
        private readonly GeneralReleaseJsonModel _release;

        /// <summary> Creates an installer for an isolated directory. </summary>
        /// <param name="instance">The installation path.</param>
        /// <param name="http">The fake HTTP factory.</param>
        /// <param name="release">The release metadata.</param>
        public TestInstaller(IInstallable instance, IHttpClientFactory http, GeneralReleaseJsonModel release)
            : base(instance, new FilesDownloader(http, NullLogger<FilesDownloader>.Instance), new ArchiveTools(NullLogger<ArchiveTools>.Instance), NullLogger.Instance)
        {
            _release = release;
        }

        /// <summary> Gets or sets whether release lookup fails. </summary>
        public bool LookupFailure { get; set; }

        /// <summary> Gets or sets an action after extraction. </summary>
        public Action? AfterExtraction { get; set; }

        /// <inheritdoc />
        protected override void Backup(string installFolder) { }

        /// <inheritdoc />
        protected override void PostInstall(string installFolder) => AfterExtraction?.Invoke();

        /// <inheritdoc />
        public override void Uninstall() => throw new InvalidOperationException("Installation must never call Uninstall.");

        /// <inheritdoc />
        public override Task<GeneralReleaseJsonModel?> GetRelease(CancellationToken cancellationToken = default) =>
            LookupFailure ? throw new IOException("Release lookup failed") : Task.FromResult<GeneralReleaseJsonModel?>(_release);
    }
}
