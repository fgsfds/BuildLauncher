using System.Security.Cryptography;

namespace Core.Client.Helpers;

/// <summary> Verifies release files using streaming SHA-256. </summary>
public static class FileHash
{
    /// <summary> Checks an optional SHA-256 digest, accepting the release API prefix. </summary>
    /// <param name="path">The downloaded file.</param>
    /// <param name="expected">The expected digest, or null when none was published.</param>
    /// <param name="cancellationToken">Cancels hashing.</param>
    /// <returns>Whether the digest matches or no digest was supplied.</returns>
    public static async Task<bool> MatchesAsync(string path, string? expected, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (expected is null)
        {
            return true;
        }

        const string Prefix = "sha256:";
        if (expected.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            expected = expected[Prefix.Length..];
        }

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        await using var streamScope = stream.ConfigureAwait(false);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        return expected.Equals(Convert.ToHexString(hash), StringComparison.OrdinalIgnoreCase);
    }
}
