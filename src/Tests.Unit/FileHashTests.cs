using System.Security.Cryptography;
using Core.Client.Helpers;

namespace Tests.Unit;

/// <summary> Verifies release integrity checks and cancellation without leaving files locked. </summary>
public sealed class FileHashTests : IDisposable
{
    /// <summary> The isolated downloaded file. </summary>
    private readonly string _path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    /// <summary> Creates a payload larger than the hashing buffer. </summary>
    public FileHashTests() => File.WriteAllBytes(_path, Enumerable.Range(0, 200000).Select(x => (byte)(x % 251)).ToArray());

    /// <summary> Deletes only this test's file. </summary>
    public void Dispose() => File.Delete(_path);

    /// <summary> Accepts API prefixes and digest casing and releases the input handle. </summary>
    /// <param name="prefix">The API digest prefix.</param>
    [Theory]
    [InlineData("")]
    [InlineData("sha256:")]
    [InlineData("SHA256:")]
    public async Task MatchesAsync_ValidDigest_AcceptsPrefixAndCasing(string prefix)
    {
        var digest = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(_path, TestContext.Current.CancellationToken))).ToLowerInvariant();
        Assert.True(await FileHash.MatchesAsync(_path, prefix + digest, TestContext.Current.CancellationToken));
        using var exclusive = File.Open(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.True(exclusive.CanWrite);
    }

    /// <summary> Modified downloads cannot pass a previously valid digest. </summary>
    [Fact]
    public async Task MatchesAsync_ModifiedPayload_RejectsDigest()
    {
        var digest = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(_path, TestContext.Current.CancellationToken)));
        await File.AppendAllTextAsync(_path, "tampered", TestContext.Current.CancellationToken);
        Assert.False(await FileHash.MatchesAsync(_path, digest, TestContext.Current.CancellationToken));
    }

    /// <summary> Cancellation takes precedence even when a release has no published hash. </summary>
    /// <param name="hash">The optional digest.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("bad")]
    public async Task MatchesAsync_PreCanceled_Throws(string? hash)
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => FileHash.MatchesAsync(_path, hash, cancellation.Token));
        using var exclusive = File.Open(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.True(exclusive.CanRead);
    }
}
