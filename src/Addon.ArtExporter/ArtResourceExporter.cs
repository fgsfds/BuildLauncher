using System.Globalization;
using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Addon.ArtExporter;

/// <summary>Exports validated ART and optional Blood surfaces into a fresh PNG/DEF directory.</summary>
public static class ArtResourceExporter
{
    /// <summary>Creates an export only after validating every source resource.</summary>
    /// <param name="options">The source, palette, destination and DEF options.</param>
    /// <returns>Input provenance and completed export counts.</returns>
    /// <exception cref="ArgumentException">A path or filename is unsuitable.</exception>
    /// <exception cref="IOException">The destination exists or file access fails.</exception>
    /// <exception cref="InvalidDataException">An input is malformed or unsupported.</exception>
    public static ExportReport Export(ExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var source = NormalizePath(options.InputDirectory);
        var destination = NormalizePath(options.OutputDirectory);
        ValidatePaths(source, destination, options.DefFileName);
        var files = Directory.GetFiles(source).Order(StringComparer.Ordinal).ToArray();
        if (files.Any(path => Path.GetFileName(path).Equals("VOXEL.DAT", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidDataException("VOXEL.DAT conversion into a separate DEF is not implemented yet.");
        }

        var paletteBytes = ReadInput(options.PalettePath);
        ValidatePalette(paletteBytes);
        var palette = paletteBytes[..768];
        var inputs = new List<ExportInput> { DescribeInput("palette", options.PalettePath, paletteBytes) };
        var artFiles = files.Where(path => Path.GetExtension(path).Equals(".art", StringComparison.OrdinalIgnoreCase)).ToArray();
        if (artFiles.Length == 0)
        {
            throw new InvalidDataException("The input directory contains no ART files.");
        }

        var slots = new SortedDictionary<int, ArtTile>();
        foreach (var path in artFiles)
        {
            var data = ReadInput(path);
            inputs.Add(DescribeInput("ART", path, data));
            foreach (var tile in ArtReader.Read(data, Path.GetFileName(path)))
            {
                if (!slots.TryAdd(tile.Id, tile))
                {
                    throw new InvalidDataException($"{Path.GetFileName(path)}: overlapping ART tile ID {tile.Id}.");
                }
            }
        }

        var tiles = slots.Values.Where(tile => tile.HasPixels).ToArray();
        if (tiles.Length == 0)
        {
            throw new InvalidDataException("The ART files contain no nonempty tiles.");
        }

        var surfacePath = options.SurfacePath ?? FindSurface(files);
        var surfaces = surfacePath is null ? [] : ReadInput(surfacePath);
        if (surfacePath is not null)
        {
            if (surfaces.Length <= tiles[^1].Id)
            {
                throw new InvalidDataException($"{Path.GetFileName(surfacePath)}: the surface table does not cover exported tile {tiles[^1].Id}.");
            }

            inputs.Add(DescribeInput("surface", surfacePath, surfaces));
        }

        var definitions = BuildDefinitions(tiles, surfaces, options.IncludeCrc);
        var exportedIds = tiles.Select(tile => tile.Id).ToHashSet();
        var report = new ExportReport
        {
            ExporterVersion = typeof(ArtResourceExporter).Assembly.GetName().Version?.ToString() ?? "unknown",
            ArtFileCount = artFiles.Length,
            TileCount = tiles.Length,
            EmptyTileCount = slots.Count - tiles.Length,
            AnimationCount = tiles.Count(tile => tile.AnimationType != 0),
            OmittedSurfaceEntries = surfaces.Where((value, index) => value != 0 && !exportedIds.Contains(index)).Count(),
            IncludeCrc = options.IncludeCrc,
            DefFileName = options.DefFileName,
            Inputs = inputs.AsReadOnly()
        };
        Publish(destination, options.DefFileName, definitions, tiles, palette, report);
        return report;
    }

    /// <summary>Normalizes a nonempty filesystem path.</summary>
    /// <param name="path">The supplied path.</param>
    /// <returns>The normalized absolute path.</returns>
    private static string NormalizePath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
    }

    /// <summary>Rejects conflicting destinations and unsafe portable DEF filenames.</summary>
    /// <param name="source">The input directory.</param>
    /// <param name="destination">The new output directory.</param>
    /// <param name="defName">The root DEF filename.</param>
    private static void ValidatePaths(string source, string destination, string defName)
    {
        if (!Directory.Exists(source))
        {
            throw new DirectoryNotFoundException($"Input directory does not exist: {source}");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(defName);
        if (!defName.EndsWith(".def", StringComparison.OrdinalIgnoreCase) ||
            defName.IndexOfAny(['/', '\\', ':', '"', '<', '>', '|', '?', '*']) >= 0 ||
            defName.Any(char.IsControl) || defName.Trim() != defName || defName.Length > 128 || IsReservedFileName(defName))
        {
            throw new ArgumentException("The DEF name must be a portable root filename ending in .def.", nameof(defName));
        }

        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (IsWithin(source, destination, comparison) || IsWithin(destination, source, comparison))
        {
            throw new ArgumentException("Input and output must be separate directories; neither may contain the other.");
        }

        if (Directory.Exists(destination) || File.Exists(destination))
        {
            throw new IOException($"Output already exists; select a new directory: {destination}");
        }

        var parent = Path.GetDirectoryName(destination);
        if (parent is null || !Directory.Exists(parent))
        {
            throw new DirectoryNotFoundException("The output's parent directory must already exist.");
        }

        RejectLinkedAncestors(source);
        RejectLinkedAncestors(parent);
    }

    /// <summary>Rejects Windows device names even when converting on another OS.</summary>
    /// <param name="fileName">The proposed portable filename.</param>
    /// <returns>Whether the filename has a reserved device stem.</returns>
    private static bool IsReservedFileName(string fileName)
    {
        var stem = fileName.Split('.')[0].TrimEnd().ToUpperInvariant();
        return stem is "CON" or "PRN" or "AUX" or "NUL" ||
               (stem.Length == 4 && stem[..3] is "COM" or "LPT" &&
                stem[3] is >= '0' and <= '9' or '\u00B9' or '\u00B2' or '\u00B3');
    }

    /// <summary>Checks containment using a separator boundary.</summary>
    /// <param name="parent">The potential ancestor.</param>
    /// <param name="child">The potential descendant.</param>
    /// <param name="comparison">The platform's path comparison.</param>
    /// <returns>Whether the child is the parent or lies beneath it.</returns>
    private static bool IsWithin(string parent, string child, StringComparison comparison) =>
        child.Equals(parent, comparison) || child.StartsWith(Path.EndsInDirectorySeparator(parent) ? parent : parent + Path.DirectorySeparatorChar, comparison);

    /// <summary>Rejects directory links that would make source/output isolation ambiguous.</summary>
    /// <param name="path">The existing directory path.</param>
    private static void RejectLinkedAncestors(string path)
    {
        for (var directory = new DirectoryInfo(path); directory is not null; directory = directory.Parent)
        {
            if ((directory.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                throw new ArgumentException($"Linked directories are unsupported: {directory.FullName}");
            }
        }
    }

    /// <summary>Reads an ordinary input file without following a link.</summary>
    /// <param name="path">The input path.</param>
    /// <returns>A bounded input snapshot.</returns>
    private static byte[] ReadInput(string path)
    {
        if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
        {
            throw new InvalidDataException($"Linked input files are unsupported: {Path.GetFileName(path)}");
        }

        return ArtReader.ReadBytes(path);
    }

    /// <summary>Validates a 256-color ACT file, including an optional footer.</summary>
    /// <param name="bytes">The original palette bytes.</param>
    private static void ValidatePalette(byte[] bytes)
    {
        if (bytes.Length is not (768 or 772))
        {
            throw new InvalidDataException("Palette must be a 768-byte RGB ACT file, optionally with a four-byte ACT footer.");
        }

        if (bytes.Length == 772)
        {
            var count = (bytes[768] << 8) | bytes[769];
            var transparent = (bytes[770] << 8) | bytes[771];
            if (count is not (0 or 256) || transparent is not (255 or 65535))
            {
                throw new InvalidDataException("ACT footer must describe 256 colors and transparent index 255 (or no declared transparency).");
            }
        }
    }

    /// <summary>Finds the optional surface file while rejecting case ambiguity.</summary>
    /// <param name="files">The source directory's files.</param>
    /// <returns>The surface file, or null.</returns>
    private static string? FindSurface(string[] files)
    {
        var matches = files.Where(path => Path.GetFileName(path).Equals("SURFACE.DAT", StringComparison.OrdinalIgnoreCase)).ToArray();
        return matches.Length switch
        {
            0 => null,
            1 => matches[0],
            _ => throw new InvalidDataException("Multiple SURFACE.DAT filenames differ only by case.")
        };
    }

    /// <summary>Describes the exact input bytes used, without storing absolute paths.</summary>
    /// <param name="role">The resource role.</param>
    /// <param name="path">The source filename.</param>
    /// <param name="bytes">The input snapshot.</param>
    /// <returns>Filename, size and SHA-256 provenance.</returns>
    private static ExportInput DescribeInput(string role, string path, byte[] bytes) => new()
    {
        Role = role,
        FileName = Path.GetFileName(path),
        Size = bytes.LongLength,
        Sha256 = Convert.ToHexStringLower(SHA256.HashData(bytes))
    };

    /// <summary>Builds deterministic DEF text from tile metadata and optional surfaces.</summary>
    /// <param name="tiles">The tiles sorted by ID.</param>
    /// <param name="surfaces">Surface bytes indexed by tile ID, or an empty array.</param>
    /// <param name="includeCrc">Whether to emit source pixel CRC conditions.</param>
    /// <returns>The complete DEF text.</returns>
    private static string BuildDefinitions(ArtTile[] tiles, byte[] surfaces, bool includeCrc)
    {
        var text = new StringBuilder();
        foreach (var tile in tiles)
        {
            _ = text.Append(CultureInfo.InvariantCulture, $"tilefromtexture {tile.Id} {{ file \"tiles/{tile.Id}.png\" xoffset {tile.XOffset} yoffset {tile.YOffset}");
            var extra = (tile.Metadata >> 28) & 7;
            if (extra != 0)
            {
                _ = text.Append(CultureInfo.InvariantCulture, $" extra {extra}");
            }

            var surface = surfaces.Length == 0 ? 0 : surfaces[tile.Id];
            if (surface > 14)
            {
                throw new InvalidDataException($"Tile {tile.Id}: unsupported Blood surface value {surface}.");
            }

            if (surface != 0)
            {
                _ = text.Append(CultureInfo.InvariantCulture, $" surface {surface}");
            }

            if (includeCrc)
            {
                _ = text.Append(CultureInfo.InvariantCulture, $" ifcrc 0x{Crc32.HashToUInt32(tile.Pixels):X8}");
            }

            _ = text.Append(" }\r\n");
            if (tile.AnimationType == 0)
            {
                continue;
            }

            var frames = (int)(tile.Metadata & 63);
            var end = tile.AnimationType == 3 ? tile.Id - frames : tile.Id + frames;
            if (end is < 0 or > 65535)
            {
                throw new InvalidDataException($"Tile {tile.Id}: animation endpoint {end} is outside the supported tile range.");
            }

            var speed = (tile.Metadata >> 24) & 15;
            _ = text.Append(CultureInfo.InvariantCulture, $"animtilerange {tile.Id} {end} {speed} {tile.AnimationType}\r\n");
        }

        return text.ToString();
    }

    /// <summary>Publishes a complete export from an owned sibling staging directory.</summary>
    /// <param name="destination">The absent final output directory.</param>
    /// <param name="defName">The root DEF filename.</param>
    /// <param name="definitions">The prepared DEF text.</param>
    /// <param name="tiles">The validated images.</param>
    /// <param name="palette">The validated palette.</param>
    /// <param name="report">The conversion provenance.</param>
    private static void Publish(string destination, string defName, string definitions, ArtTile[] tiles, byte[] palette, ExportReport report)
    {
        var parent = Path.GetDirectoryName(destination)!;
        var stage = Path.GetFullPath(Path.Combine(parent, $".bl-art-export-{Guid.NewGuid():N}"));
        if (Path.GetDirectoryName(stage) != parent || Directory.Exists(stage) || File.Exists(stage))
        {
            throw new IOException("Unable to create an isolated staging directory.");
        }

        _ = Directory.CreateDirectory(stage);
        try
        {
            var tileDirectory = Path.Combine(stage, "tiles");
            _ = Directory.CreateDirectory(tileDirectory);
            foreach (var tile in tiles)
            {
                IndexedPngWriter.Write(Path.Combine(tileDirectory, tile.Id.ToString(CultureInfo.InvariantCulture) + ".png"), tile, palette);
            }

            File.WriteAllText(Path.Combine(stage, defName), definitions, new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(stage, "conversion-report.json"), JsonSerializer.Serialize(report, ExportJsonContext.Default.ExportReport), new UTF8Encoding(false));
            Directory.Move(stage, destination);
        }
        finally
        {
            if (Directory.Exists(stage))
            {
                try
                {
                    Directory.Delete(stage, recursive: true);
                }
                catch (IOException)
                {
                    Console.Error.WriteLine($"Staging cleanup failed; remove this export directory manually: {stage}");
                }
                catch (UnauthorizedAccessException)
                {
                    Console.Error.WriteLine($"Staging cleanup denied; remove this export directory manually: {stage}");
                }
            }
        }
    }
}
