using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Addon.ArtExporter;
using Exporter = Addon.ArtExporter.ArtResourceExporter;

namespace Tests.Unit;

/// <summary>
/// Verifies standalone resource exports using isolated synthetic addon inputs.
/// </summary>
public sealed class ArtResourceExporterTests : IDisposable
{
    /// <summary>The unique directory owned by this test instance.</summary>
    private readonly string _rootPath = Path.Combine(Path.GetTempPath(), $"BuildLauncher-ArtResourceExporterTests-{Guid.NewGuid():N}");

    /// <summary>The preserved source directory.</summary>
    private readonly string _inputPath;

    /// <summary>The fresh export directory.</summary>
    private readonly string _outputPath;

    /// <summary>The source palette file.</summary>
    private readonly string _palettePath;

    /// <summary>The original RGB palette in index order.</summary>
    private readonly byte[] _palette = Enumerable.Range(0, 256).SelectMany(index => new[] { (byte)index, (byte)(255 - index), (byte)(index ^ 90) }).ToArray();

    /// <summary>Creates an isolated fixture and deterministic RGB palette.</summary>
    public ArtResourceExporterTests()
    {
        _inputPath = Path.Combine(_rootPath, "source");
        _outputPath = Path.Combine(_rootPath, "output");
        _palettePath = Path.Combine(_rootPath, "game.act");
        _ = Directory.CreateDirectory(_inputPath);
        File.WriteAllBytes(_palettePath, _palette);
    }

    /// <summary>Removes only files owned by the test instance.</summary>
    public void Dispose()
    {
        Directory.Delete(_rootPath, true);
    }

    /// <summary>Preserves nonsquare pixels, palette colors, transparency, offsets and surfaces.</summary>
    [Fact]
    public void Export_NonsquareArt_PreservesResourcesAndSource()
    {
        var metadata = (uint)(254 << 8) | (3u << 16);
        WriteArt("unrelated-name.ART", 100, [new(2, 3, metadata, [10, 20, 30, 40, 255, 60]), new(0, 0, 0, [])]);
        var surface = new byte[102];
        surface[100] = 3;
        surface[1] = 9;
        File.WriteAllBytes(Path.Combine(_inputPath, "surface.dat"), surface);
        var sourceBytes = SnapshotSource();

        var report = Exporter.Export(CreateOptions());

        Assert.Equal(1, report.ArtFileCount);
        Assert.Equal(1, report.TileCount);
        Assert.Equal(1, report.EmptyTileCount);
        Assert.Equal(0, report.AnimationCount);
        Assert.Equal(1, report.OmittedSurfaceEntries);
        var png = ReadPng(Path.Combine(_outputPath, "tiles", "100.png"));
        Assert.Equal(2, png.Width);
        Assert.Equal(3, png.Height);
        Assert.Equal(new byte[] { 11, 41, 21, 0, 31, 61 }, png.Pixels);
        Assert.Equal(_palette[^3..].Concat(_palette[..^3]).ToArray(), png.Palette);
        Assert.NotEmpty(png.Transparency);
        Assert.Equal(0, png.Transparency[0]);
        Assert.All(png.Transparency.Skip(1), value => Assert.Equal(255, value));
        var definition = File.ReadAllText(Path.Combine(_outputPath, "resources.def"));
        var block = TextureBlock(definition, 100);
        Assert.Contains("tiles/100.png", block, StringComparison.Ordinal);
        Assert.Matches(@"\bxoffset\s+-2\b", block);
        Assert.Matches(@"\byoffset\s+3\b", block);
        Assert.Matches(@"\bsurface\s+3\b", block);
        Assert.DoesNotContain("ifcrc", definition, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(Path.Combine(_outputPath, "tiles", "101.png")));
        AssertSourceUnchanged(sourceBytes);
    }

    /// <summary>Preserves each animation direction and Blood view metadata.</summary>
    [Fact]
    public void Export_AnimationsAndViewBits_ProducesMatchingDef()
    {
        WriteArt("TILES001.ART", 20,
        [
            new(1, 1, 2u | (2u << 6) | (4u << 24) | (5u << 28), [1]),
            new(1, 1, 3u | (3u << 6) | (2u << 24), [2]),
            new(1, 1, 1u | (1u << 6) | (7u << 24), [3])
        ]);

        var report = Exporter.Export(CreateOptions());

        Assert.Equal(3, report.AnimationCount);
        var definition = File.ReadAllText(Path.Combine(_outputPath, "resources.def"));
        Assert.Matches(@"\banimtilerange\s+20\s+22\s+4\s+2\b", definition);
        Assert.Matches(@"\banimtilerange\s+21\s+18\s+2\s+3\b", definition);
        Assert.Matches(@"\banimtilerange\s+22\s+23\s+7\s+1\b", definition);
        Assert.Matches(@"\bextra\s+5\b", TextureBlock(definition, 20));
        Assert.DoesNotMatch(@"\bsurface\b", definition);
    }

    /// <summary>Checks CRC against the established CRC32 test vector.</summary>
    [Fact]
    public void Export_IncludeCrc_WritesSourcePixelCrcAndClosedBlock()
    {
        WriteArt("crc.art", 4, [new(9, 1, 0, Encoding.ASCII.GetBytes("123456789"))]);

        _ = Exporter.Export(CreateOptions() with { IncludeCrc = true, DefFileName = "custom.def" });

        var definition = File.ReadAllText(Path.Combine(_outputPath, "custom.def"));
        var block = TextureBlock(definition, 4);
        Assert.Matches(new Regex(@"\bifcrc\s+0xcbf43926\b", RegexOptions.IgnoreCase), block);
        Assert.EndsWith("}", block.Trim(), StringComparison.Ordinal);
    }

    /// <summary>Supports prefixed ART headers and a valid ACT footer.</summary>
    [Fact]
    public void Export_BuildArtPrefixAndActFooter_AreAccepted()
    {
        WriteArt("prefixed.art", 8, [new(1, 1, 0, [255])], buildPrefix: true);
        File.WriteAllBytes(_palettePath, _palette.Concat(new byte[] { 1, 0, 255, 255 }).ToArray());

        var report = Exporter.Export(CreateOptions());

        Assert.Equal(1, report.TileCount);
        Assert.Equal(new byte[] { 0 }, ReadPng(Path.Combine(_outputPath, "tiles", "8.png")).Pixels);
    }

    /// <summary>Uses header IDs across several ART files and honors an explicit surface table.</summary>
    [Fact]
    public void Export_MultipleArtAndExplicitSurface_UsesSourceIds()
    {
        WriteArt("z-first.art", 4, [new(1, 1, 0, [2])]);
        WriteArt("a-second.art", 12, [new(1, 1, 0, [7])]);
        var surfacePath = Path.Combine(_rootPath, "override.dat");
        var surfaces = new byte[13];
        surfaces[12] = 5;
        File.WriteAllBytes(surfacePath, surfaces);

        var report = Exporter.Export(CreateOptions() with { SurfacePath = surfacePath });

        Assert.Equal(2, report.ArtFileCount);
        Assert.Equal(2, report.TileCount);
        Assert.Equal(new byte[] { 3 }, ReadPng(Path.Combine(_outputPath, "tiles", "4.png")).Pixels);
        Assert.Equal(new byte[] { 8 }, ReadPng(Path.Combine(_outputPath, "tiles", "12.png")).Pixels);
        var definition = File.ReadAllText(Path.Combine(_outputPath, "resources.def"));
        Assert.DoesNotMatch(@"\bsurface\b", TextureBlock(definition, 4));
        Assert.Matches(@"\bsurface\s+5\b", TextureBlock(definition, 12));
    }

    /// <summary>Rejects malformed input before leaving any export directory.</summary>
    /// <param name="failure">The malformed resource to create.</param>
    [Theory]
    [InlineData("version")]
    [InlineData("header")]
    [InlineData("arrays")]
    [InlineData("pixels")]
    [InlineData("trailing")]
    [InlineData("overlap")]
    [InlineData("negative")]
    [InlineData("range")]
    [InlineData("negative-id")]
    [InlineData("surface")]
    [InlineData("voxel")]
    [InlineData("palette")]
    [InlineData("footer")]
    public void Export_InvalidResource_PreservesSourceAndLeavesNoOutput(string failure)
    {
        WriteArt("first.art", 4, [new(1, 1, 0, [1])]);
        var artPath = Path.Combine(_inputPath, "first.art");
        var artBytes = File.ReadAllBytes(artPath);
        switch (failure)
        {
            case "version":
                BinaryPrimitives.WriteInt32LittleEndian(artBytes, 2);
                File.WriteAllBytes(artPath, artBytes);
                break;
            case "header":
                File.WriteAllBytes(artPath, artBytes[..8]);
                break;
            case "arrays":
                File.WriteAllBytes(artPath, artBytes[..18]);
                break;
            case "pixels":
                File.WriteAllBytes(artPath, artBytes[..^1]);
                break;
            case "trailing":
                File.WriteAllBytes(artPath, artBytes.Concat(new byte[] { 0 }).ToArray());
                break;
            case "overlap":
                WriteArt("second.art", 4, [new(1, 1, 0, [2])]);
                break;
            case "negative":
                BinaryPrimitives.WriteInt16LittleEndian(artBytes.AsSpan(16), -1);
                File.WriteAllBytes(artPath, artBytes);
                break;
            case "range":
                BinaryPrimitives.WriteInt32LittleEndian(artBytes.AsSpan(12), 3);
                File.WriteAllBytes(artPath, artBytes);
                break;
            case "negative-id":
                BinaryPrimitives.WriteInt32LittleEndian(artBytes.AsSpan(8), -1);
                File.WriteAllBytes(artPath, artBytes);
                break;
            case "surface":
                File.WriteAllBytes(Path.Combine(_inputPath, "SURFACE.DAT"), new byte[4]);
                break;
            case "voxel":
                File.WriteAllBytes(Path.Combine(_inputPath, "VOXEL.DAT"), [255, 255]);
                break;
            case "palette":
                File.WriteAllBytes(_palettePath, _palette[..^1]);
                break;
            case "footer":
                File.WriteAllBytes(_palettePath, _palette.Concat(new byte[] { 1, 1, 255, 255 }).ToArray());
                break;
            default:
                throw new ArgumentException("Unknown fixture", nameof(failure));
        }

        var sourceBytes = SnapshotSource();

        _ = Assert.Throws<InvalidDataException>(() => Exporter.Export(CreateOptions()));

        Assert.False(Directory.Exists(_outputPath));
        AssertSourceUnchanged(sourceBytes);
    }

    /// <summary>Preserves existing output content instead of overwriting it.</summary>
    [Fact]
    public void Export_ExistingOutput_PreservesUserFiles()
    {
        WriteArt("first.art", 4, [new(1, 1, 0, [1])]);
        _ = Directory.CreateDirectory(_outputPath);
        var marker = Path.Combine(_outputPath, "user.txt");
        File.WriteAllText(marker, "Existing user content");

        _ = Assert.Throws<IOException>(() => Exporter.Export(CreateOptions()));

        Assert.Equal("Existing user content", File.ReadAllText(marker));
        Assert.Single(Directory.EnumerateFileSystemEntries(_outputPath));
    }

    /// <summary>Rejects output directories inside the source tree.</summary>
    [Fact]
    public void Export_NestedOutput_IsRejectedWithoutChangingSource()
    {
        WriteArt("first.art", 4, [new(1, 1, 0, [1])]);
        var nestedOutput = Path.Combine(_inputPath, "converted");
        var sourceBytes = SnapshotSource();

        _ = Assert.Throws<ArgumentException>(() => Exporter.Export(CreateOptions() with { OutputDirectory = nestedOutput }));

        Assert.False(Directory.Exists(nestedOutput));
        AssertSourceUnchanged(sourceBytes);
    }

    /// <summary>Rejects DEF names which escape the selected output directory.</summary>
    /// <param name="filename">The unsafe DEF filename.</param>
    [Theory]
    [InlineData("CON.def")]
    [InlineData("prn .def")]
    [InlineData("COM1.def")]
    [InlineData("../escaped.def")]
    [InlineData("..\\escaped.def")]
    public void Export_UnsafeDefName_IsRejectedWithoutOutput(string filename)
    {
        WriteArt("first.art", 4, [new(1, 1, 0, [1])]);

        _ = Assert.Throws<ArgumentException>(() => Exporter.Export(CreateOptions() with { DefFileName = filename }));

        Assert.False(Directory.Exists(_outputPath));
        Assert.False(File.Exists(Path.Combine(_rootPath, "escaped.def")));
    }

    /// <summary>Rejects an absolute DEF path.</summary>
    [Fact]
    public void Export_AbsoluteDefName_IsRejectedWithoutOutput()
    {
        WriteArt("first.art", 4, [new(1, 1, 0, [1])]);
        var absoluteName = Path.Combine(_rootPath, "escaped.def");

        _ = Assert.Throws<ArgumentException>(() => Exporter.Export(CreateOptions() with { DefFileName = absoluteName }));

        Assert.False(Directory.Exists(_outputPath));
        Assert.False(File.Exists(absoluteName));
    }

    /// <summary>Creates default public exporter options for the isolated fixture.</summary>
    /// <returns>The options targeting a fresh output directory.</returns>
    private ExportOptions CreateOptions() => new()
    {
        InputDirectory = _inputPath,
        OutputDirectory = _outputPath,
        PalettePath = _palettePath
    };

    /// <summary>Captures source bytes independently of exporter implementation.</summary>
    /// <returns>A source-relative path to bytes snapshot.</returns>
    private Dictionary<string, byte[]> SnapshotSource() => Directory.EnumerateFiles(_inputPath, "*", SearchOption.AllDirectories)
        .ToDictionary(path => Path.GetRelativePath(_inputPath, path), File.ReadAllBytes);

    /// <summary>Checks that no source content or source file set changed.</summary>
    /// <param name="snapshot">The pre-export source snapshot.</param>
    private void AssertSourceUnchanged(Dictionary<string, byte[]> snapshot)
    {
        var current = SnapshotSource();
        Assert.Equal(snapshot.Keys.Order(), current.Keys.Order());
        foreach (var (path, bytes) in snapshot)
        {
            Assert.Equal(bytes, current[path]);
        }
    }

    /// <summary>Writes independently constructed standard ART fixture bytes.</summary>
    /// <param name="filename">The root source filename.</param>
    /// <param name="start">The first tile ID.</param>
    /// <param name="tiles">The slot dimensions, metadata and column-major pixel bytes.</param>
    /// <param name="buildPrefix">Whether to add the supported BUILDART prefix.</param>
    private void WriteArt(string filename, int start, IReadOnlyList<ArtTile> tiles, bool buildPrefix = false)
    {
        using var stream = File.Create(Path.Combine(_inputPath, filename));
        using var writer = new BinaryWriter(stream, Encoding.ASCII);
        if (buildPrefix)
        {
            writer.Write(Encoding.ASCII.GetBytes("BUILDART"));
        }

        writer.Write(1);
        writer.Write(tiles.Count);
        writer.Write(start);
        writer.Write(start + tiles.Count - 1);
        foreach (var tile in tiles)
        {
            writer.Write(tile.Width);
        }

        foreach (var tile in tiles)
        {
            writer.Write(tile.Height);
        }

        foreach (var tile in tiles)
        {
            writer.Write(tile.Metadata);
        }

        foreach (var tile in tiles)
        {
            writer.Write(tile.Pixels);
        }
    }

    /// <summary>Finds a balanced texture block by its tile ID.</summary>
    /// <param name="definition">The generated DEF text.</param>
    /// <param name="tile">The expected tile ID.</param>
    /// <returns>The complete texture block.</returns>
    private static string TextureBlock(string definition, int tile)
    {
        var match = Regex.Match(definition, $@"\btilefromtexture\s+{tile}\s*\{{[^{{}}]*\}}", RegexOptions.Singleline);
        Assert.True(match.Success, $"Missing or unclosed texture block for tile {tile}");
        return match.Value;
    }

    /// <summary>Decodes indexed PNG chunks and all standard byte filters.</summary>
    /// <param name="path">The exported PNG path.</param>
    /// <returns>The decoded dimensions, palette, transparency and row-major indices.</returns>
    private static PngData ReadPng(string path)
    {
        var bytes = File.ReadAllBytes(path);
        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, bytes[..8]);
        var width = 0;
        var height = 0;
        var palette = Array.Empty<byte>();
        var transparency = Array.Empty<byte>();
        using var compressed = new MemoryStream();
        for (var offset = 8; offset < bytes.Length;)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(offset));
            var name = Encoding.ASCII.GetString(bytes, offset + 4, 4);
            var chunk = bytes.AsSpan(offset + 8, length);
            switch (name)
            {
                case "IHDR":
                    width = BinaryPrimitives.ReadInt32BigEndian(chunk);
                    height = BinaryPrimitives.ReadInt32BigEndian(chunk[4..]);
                    Assert.Equal(8, chunk[8]);
                    Assert.Equal(3, chunk[9]);
                    Assert.Equal(0, chunk[12]);
                    break;
                case "PLTE":
                    palette = chunk.ToArray();
                    break;
                case "tRNS":
                    transparency = chunk.ToArray();
                    break;
                case "IDAT":
                    compressed.Write(chunk);
                    break;
            }

            offset += length + 12;
        }

        compressed.Position = 0;
        using var inflater = new ZLibStream(compressed, CompressionMode.Decompress);
        using var decompressed = new MemoryStream();
        inflater.CopyTo(decompressed);
        var scanlines = decompressed.ToArray();
        Assert.Equal((width + 1) * height, scanlines.Length);
        var pixels = new byte[width * height];
        for (var y = 0; y < height; y++)
        {
            var filter = scanlines[y * (width + 1)];
            for (var x = 0; x < width; x++)
            {
                var left = x == 0 ? 0 : pixels[y * width + x - 1];
                var above = y == 0 ? 0 : pixels[(y - 1) * width + x];
                var upperLeft = x == 0 || y == 0 ? 0 : pixels[(y - 1) * width + x - 1];
                var predictor = filter switch
                {
                    0 => 0,
                    1 => left,
                    2 => above,
                    3 => (left + above) / 2,
                    4 => Paeth(left, above, upperLeft),
                    _ => throw new InvalidDataException("Unexpected PNG filter")
                };
                pixels[y * width + x] = unchecked((byte)(scanlines[y * (width + 1) + x + 1] + predictor));
            }
        }

        return new(width, height, pixels, palette, transparency);
    }

    /// <summary>Decodes the standard PNG Paeth predictor.</summary>
    /// <param name="left">The previous pixel.</param>
    /// <param name="above">The previous row's pixel.</param>
    /// <param name="upperLeft">The previous row's previous pixel.</param>
    /// <returns>The closest neighbor to the linear estimate.</returns>
    private static int Paeth(int left, int above, int upperLeft)
    {
        var estimate = left + above - upperLeft;
        var leftDistance = Math.Abs(estimate - left);
        var aboveDistance = Math.Abs(estimate - above);
        var diagonalDistance = Math.Abs(estimate - upperLeft);
        return leftDistance <= aboveDistance && leftDistance <= diagonalDistance ? left : aboveDistance <= diagonalDistance ? above : upperLeft;
    }

    /// <summary>An independently constructed ART slot.</summary>
    /// <param name="Width">The tile width.</param>
    /// <param name="Height">The tile height.</param>
    /// <param name="Metadata">The raw picanm metadata.</param>
    /// <param name="Pixels">The column-major source indices.</param>
    private sealed record ArtTile(short Width, short Height, uint Metadata, byte[] Pixels);

    /// <summary>A decoded indexed PNG for observable output assertions.</summary>
    /// <param name="Width">The PNG width.</param>
    /// <param name="Height">The PNG height.</param>
    /// <param name="Pixels">The decoded row-major pixel indices.</param>
    /// <param name="Palette">The RGB PLTE bytes.</param>
    /// <param name="Transparency">The optional tRNS bytes.</param>
    private sealed record PngData(int Width, int Height, byte[] Pixels, byte[] Palette, byte[] Transparency);
}
