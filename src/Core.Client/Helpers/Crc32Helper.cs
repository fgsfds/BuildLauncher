using System.Buffers;
using System.Buffers.Binary;
using System.IO.Hashing;

namespace Core.Client.Helpers;

/// <summary>
///     Provides utility methods for computing CRC-32 hashes of files.
/// </summary>
public static class Crc32Helper
{
    /// <summary>
    ///     Computes the CRC-32 hash of a file.
    /// </summary>
    /// <param name="path">Absolute path to the file.</param>
    /// <returns>The CRC-32 hash as a signed 64-bit integer.</returns>
    public static long GetCrc32(string path)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);

        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 0, FileOptions.SequentialScan);
            var hasher = new Crc32();

            int bytesRead;

            while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
            {
                hasher.Append(buffer.AsSpan(0, bytesRead));
            }

            Span<byte> hashBytes = stackalloc byte[4];
            hasher.GetCurrentHash(hashBytes);

            return BinaryPrimitives.ReadUInt32LittleEndian(hashBytes);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    ///     Computes the CRC-32 hash of a file and returns it as a hexadecimal string.
    /// </summary>
    /// <param name="path">Absolute path to the file.</param>
    /// <returns>The CRC-32 hash as a hexadecimal string prefixed with "0x".</returns>
    public static string GetCrc32Hex(string path) => $"0x{GetCrc32(path):X8}";
}
