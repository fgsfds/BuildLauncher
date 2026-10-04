namespace Addon.ArtExporter;

/// <summary>Provenance and counts for a completed resource export.</summary>
public sealed record ExportReport
{
    /// <summary>Gets the exporter assembly version.</summary>
    public required string ExporterVersion { get; init; }

    /// <summary>Gets the number of source ART files.</summary>
    public required int ArtFileCount { get; init; }

    /// <summary>Gets the number of exported nonempty tiles.</summary>
    public required int TileCount { get; init; }

    /// <summary>Gets the number of skipped empty ART slots.</summary>
    public required int EmptyTileCount { get; init; }

    /// <summary>Gets the number of exported animation definitions.</summary>
    public required int AnimationCount { get; init; }

    /// <summary>Gets nonzero surface entries omitted because their tiles were not exported.</summary>
    public required int OmittedSurfaceEntries { get; init; }

    /// <summary>Gets whether source CRC conditions were emitted.</summary>
    public required bool IncludeCrc { get; init; }

    /// <summary>Gets the emitted root DEF filename.</summary>
    public required string DefFileName { get; init; }

    /// <summary>Gets the PNG pixel-index representation.</summary>
    public string IndexMapping { get; init; } = "pngIndex = (artIndex + 1) % 256; transparentIndex = 0";

    /// <summary>Gets the hashed inputs used for the conversion.</summary>
    public required IReadOnlyList<ExportInput> Inputs { get; init; }
}

/// <summary>A hashed input snapshot without machine-specific absolute paths.</summary>
public sealed record ExportInput
{
    /// <summary>Gets the input's role: ART, palette or surface.</summary>
    public required string Role { get; init; }

    /// <summary>Gets the source filename.</summary>
    public required string FileName { get; init; }

    /// <summary>Gets the size of the actual bytes read.</summary>
    public required long Size { get; init; }

    /// <summary>Gets their SHA-256 hash.</summary>
    public required string Sha256 { get; init; }
}
