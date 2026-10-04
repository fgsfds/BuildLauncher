namespace Addon.ArtExporter;

/// <summary>Options for exporting an addon's ART resources without modifying the source.</summary>
public sealed record ExportOptions
{
    /// <summary>Gets the directory containing the input ART files.</summary>
    public required string InputDirectory { get; init; }

    /// <summary>Gets the new directory to publish after a successful export.</summary>
    public required string OutputDirectory { get; init; }

    /// <summary>Gets the path to a 256-color RGB ACT palette.</summary>
    public required string PalettePath { get; init; }

    /// <summary>Gets the output DEF filename at the export root.</summary>
    public string DefFileName { get; init; } = "resources.def";

    /// <summary>Gets an explicit SURFACE.DAT path, or null for automatic discovery.</summary>
    public string? SurfacePath { get; init; }

    /// <summary>Gets whether each tile definition includes its source pixel CRC-32.</summary>
    public bool IncludeCrc { get; init; }
}
