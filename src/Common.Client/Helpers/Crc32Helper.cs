using System.IO.Hashing;

namespace Common.Client.Helpers;

public static class Crc32Helper
{
    public static long GetCrc32(string path)
    {
        using var fs = File.OpenRead(path);
        var hasher = new Crc32();
        var buffer = new byte[1 << 20];
        int bytesRead;

        while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
        {
            hasher.Append(new ReadOnlySpan<byte>(buffer, 0, bytesRead));
        }

        var hash = hasher.GetCurrentHash();
        var crc = BitConverter.ToUInt32(hash, 0);

        return crc;
    }


    public static string GetCrc32Hex(string path) => $"0x{GetCrc32(path):X8}";
}
