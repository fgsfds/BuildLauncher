using System.Buffers.Binary;
using System.IO.Compression;
using System.IO.Hashing;

namespace Addon.ArtExporter;

/// <summary>Writes indexed PNGs with exact palette indices and transparent index zero.</summary>
internal static class IndexedPngWriter
{
    /// <summary>Exports column-major ART pixels as a PNG without color quantization.</summary>
    /// <param name="path">The new output file.</param>
    /// <param name="tile">The decoded source tile.</param>
    /// <param name="palette">The unrotated 768-byte RGB palette.</param>
    internal static void Write(string path, ArtTile tile, byte[] palette)
    {
        using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        output.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        var header = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, tile.Width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), tile.Height);
        header[8] = 8;
        header[9] = 3;
        WriteChunk(output, "IHDR"u8, header);
        var rotatedPalette = new byte[768];
        palette.AsSpan(765, 3).CopyTo(rotatedPalette);
        palette.AsSpan(0, 765).CopyTo(rotatedPalette.AsSpan(3));
        WriteChunk(output, "PLTE"u8, rotatedPalette);
        WriteChunk(output, "tRNS"u8, [0]);
        using var compressed = new MemoryStream();
        using (var deflate = new ZLibStream(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            var row = new byte[tile.Width + 1];
            for (var y = 0; y < tile.Height; y++)
            {
                for (var x = 0; x < tile.Width; x++)
                {
                    row[x + 1] = unchecked((byte)(tile.Pixels[(x * tile.Height) + y] + 1));
                }

                deflate.Write(row);
            }
        }

        WriteChunk(output, "IDAT"u8, compressed.GetBuffer().AsSpan(0, (int)compressed.Length));
        WriteChunk(output, "IEND"u8, []);
    }

    /// <summary>Writes a PNG chunk with its standard big-endian CRC-32.</summary>
    /// <param name="output">The destination stream.</param>
    /// <param name="type">The four ASCII type bytes.</param>
    /// <param name="data">The chunk payload.</param>
    private static void WriteChunk(Stream output, ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        var number = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(number, data.Length);
        output.Write(number);
        output.Write(type);
        output.Write(data);
        var crc = new Crc32();
        crc.Append(type);
        crc.Append(data);
        BinaryPrimitives.WriteUInt32BigEndian(number, crc.GetCurrentHashAsUInt32());
        output.Write(number);
    }
}
