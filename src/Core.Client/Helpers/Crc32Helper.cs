using System.Buffers;
using System.Buffers.Binary;
using System.IO.Hashing;

namespace Core.Client.Helpers;

public static class Crc32Helper
{
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

    public static string GetCrc32Hex(string path) => $"0x{GetCrc32(path):X8}";
}
