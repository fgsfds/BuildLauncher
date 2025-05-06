using System.IO.Hashing;

namespace Common.Client.Helpers;

public static class Crc32Helper
{
    public static string GetCrc32(string path, bool isHex)
    {
        using var fs = File.OpenRead(path);
        var hasher = new Crc32();
        byte[] buffer = new byte[1 << 20];
        int bytesRead;

        while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
        {
            hasher.Append(new ReadOnlySpan<byte>(buffer, 0, bytesRead));
        }

        byte[] hash = hasher.GetCurrentHash();
        uint crc = BitConverter.ToUInt32(hash, 0);

        if (isHex)
        {
            return $"0x{crc:X8}";
        }

        return crc.ToString();
    }
}
