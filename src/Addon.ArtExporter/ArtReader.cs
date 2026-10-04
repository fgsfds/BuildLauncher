using System.Buffers.Binary;

namespace Addon.ArtExporter;

/// <summary>A decoded ART slot retaining the original column-major pixels.</summary>
/// <param name="Id">The global tile identifier.</param>
/// <param name="Width">The image width.</param>
/// <param name="Height">The image height.</param>
/// <param name="Metadata">The original picanm bits.</param>
/// <param name="Pixels">The original pixel indices.</param>
internal sealed record ArtTile(int Id, int Width, int Height, uint Metadata, byte[] Pixels)
{
    /// <summary>Gets whether the slot has image pixels.</summary>
    public bool HasPixels => Width != 0 && Height != 0;

    /// <summary>Gets the animation type.</summary>
    public int AnimationType => (int)((Metadata >> 6) & 3);

    /// <summary>Gets the signed horizontal offset.</summary>
    public int XOffset => unchecked((sbyte)(Metadata >> 8));

    /// <summary>Gets the signed vertical offset.</summary>
    public int YOffset => unchecked((sbyte)(Metadata >> 16));
}

/// <summary>Validates and decodes classic ART v1 and its BUILDART-prefixed variant.</summary>
internal static class ArtReader
{
    /// <summary>Reads bounded input bytes, rejecting excessive allocation before reading.</summary>
    /// <param name="path">The source path.</param>
    /// <returns>A snapshot of the source bytes.</returns>
    internal static byte[] ReadBytes(string path)
    {
        using var input = File.OpenRead(path);
        if (input.Length > 256 * 1024 * 1024)
        {
            throw new InvalidDataException($"{Path.GetFileName(path)}: input exceeds the 256 MiB file limit.");
        }

        var data = new byte[(int)input.Length];
        input.ReadExactly(data);
        return data;
    }

    /// <summary>Decodes every slot after checking the complete header and pixel bounds.</summary>
    /// <param name="data">The input bytes.</param>
    /// <param name="name">The source filename for errors.</param>
    /// <returns>The decoded slots.</returns>
    internal static IReadOnlyList<ArtTile> Read(byte[] data, string name)
    {
        var prefix = data.AsSpan().StartsWith("BUILDART"u8) ? 8 : 0;
        var bytes = data.AsSpan(prefix);
        if (bytes.Length < 16)
        {
            throw new InvalidDataException($"{name}: truncated ART header.");
        }

        var version = BinaryPrimitives.ReadInt32LittleEndian(bytes);
        var start = BinaryPrimitives.ReadInt32LittleEndian(bytes[8..]);
        var end = BinaryPrimitives.ReadInt32LittleEndian(bytes[12..]);
        if (version != 1 || start < 0 || end < start || end > 65535)
        {
            throw new InvalidDataException($"{name}: unsupported ART version or tile range.");
        }

        var count = end - start + 1;
        var offset = 16 + (8 * count);
        if (offset > bytes.Length)
        {
            throw new InvalidDataException($"{name}: truncated ART arrays.");
        }

        var tiles = new List<ArtTile>(count);
        for (var index = 0; index < count; index++)
        {
            var id = start + index;
            var width = BinaryPrimitives.ReadInt16LittleEndian(bytes[(16 + (2 * index))..]);
            var height = BinaryPrimitives.ReadInt16LittleEndian(bytes[(16 + (2 * count) + (2 * index))..]);
            var metadata = BinaryPrimitives.ReadUInt32LittleEndian(bytes[(16 + (4 * count) + (4 * index))..]);
            if (width < 0 || height < 0)
            {
                throw new InvalidDataException($"{name}: tile {id} has negative dimensions.");
            }

            var length = width * height;
            if (length > bytes.Length - offset)
            {
                throw new InvalidDataException($"{name}: tile {id} has truncated pixel data.");
            }

            if (length == 0 && metadata != 0)
            {
                throw new InvalidDataException($"{name}: tile {id} has unsupported metadata in an empty slot.");
            }

            if ((metadata & 0x80000000) != 0)
            {
                throw new InvalidDataException($"{name}: tile {id} has an unsupported high metadata bit.");
            }

            tiles.Add(new ArtTile(id, width, height, metadata, bytes.Slice(offset, length).ToArray()));
            offset += length;
        }

        if (offset != bytes.Length)
        {
            throw new InvalidDataException($"{name}: unexpected trailing ART bytes.");
        }

        return tiles;
    }
}
