using Core.Client.Helpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests.Unit;

/// <summary> Verifies replacement rollback independently of download and extraction. </summary>
public sealed class DirectoryReplacementTests
{
    /// <summary> Staging rejects Linux symlinks without following or deleting their external target. </summary>
    /// <param name="directoryLink">Whether the link points to a directory.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CopyExisting_LinuxLink_PreservesExternalTarget(bool directoryLink)
    {
        if (!OperatingSystem.IsLinux()) { return; }
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var live = Path.Combine(root, "live");
        var outside = Path.Combine(root, "outside");
        _ = Directory.CreateDirectory(live);
        _ = Directory.CreateDirectory(outside);
        var sentinel = Path.Combine(outside, "user.txt");
        File.WriteAllText(sentinel, "keep");
        try
        {
            if (directoryLink) { _ = Directory.CreateSymbolicLink(Path.Combine(live, "link"), outside); }
            else { _ = File.CreateSymbolicLink(Path.Combine(live, "link"), sentinel); }
            using (var replacement = new DirectoryReplacement(live, NullLogger.Instance))
            {
                Assert.Throws<IOException>(() => replacement.CopyExisting());
            }
            Assert.Equal("keep", File.ReadAllText(sentinel));
            Assert.Empty(Directory.GetDirectories(root, "*.staging-*"));
        }
        finally { Directory.Delete(root, true); }
    }

    /// <summary> A failed staging move restores the original live directory. </summary>
    [Fact]
    public void Commit_FailedMove_RestoresOriginal()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var live = Path.Combine(root, "live");
        _ = Directory.CreateDirectory(live);
        File.WriteAllText(Path.Combine(live, "original"), "keep");
        try
        {
            using var replacement = new DirectoryReplacement(live, NullLogger.Instance);
            Directory.Delete(replacement.StagingPath);
            Assert.Throws<DirectoryNotFoundException>(replacement.Commit);
            Assert.Equal("keep", File.ReadAllText(Path.Combine(live, "original")));
            Assert.Single(Directory.GetDirectories(root));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    /// <summary> Disposing an uncommitted replacement removes staging without touching live content. </summary>
    [Fact]
    public void Dispose_UncommittedReplacement_PreservesOriginal()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var live = Path.Combine(root, "live");
        _ = Directory.CreateDirectory(live);
        File.WriteAllText(Path.Combine(live, "user.cfg"), "original");
        try
        {
            string staging;
            using (var replacement = new DirectoryReplacement(live, NullLogger.Instance))
            {
                staging = replacement.StagingPath;
                replacement.CopyExisting();
                File.WriteAllText(Path.Combine(staging, "user.cfg"), "modified");
            }

            Assert.False(Directory.Exists(staging));
            Assert.Equal("original", File.ReadAllText(Path.Combine(live, "user.cfg")));
            Assert.Single(Directory.GetDirectories(root));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
