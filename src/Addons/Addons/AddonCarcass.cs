using System.Collections.Immutable;
using Core.All.Enums;
using Core.All.Interfaces;

namespace Addons.Addons;

/// <summary>
/// Intermediate representation built from an addon manifest before converting to a domain <see cref="BaseAddon"/>.
/// </summary>
internal struct AddonCarcass
{
    /// <summary>
    /// The game this addon is built for.
    /// </summary>
    public GameEnum SupportedGame { get; init; }

    /// <summary>
    /// The unique identifier of the addon.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// The display title of the addon.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Whether this is a campaign (TC), map, or mod.
    /// </summary>
    public AddonTypeEnum Type { get; init; }

    /// <summary>
    /// The version string of the addon.
    /// </summary>
    public string Version { get; init; }

    /// <summary>
    /// The author of the addon, if any.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// The release date of the addon, if any.
    /// </summary>
    public DateOnly? ReleaseDate { get; init; }

    /// <summary>
    /// A short description of the addon, if any.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Hash of the grid/thumbnail image, falls back to preview hash.
    /// </summary>
    public long? GridImageHash { get; init; }

    /// <summary>
    /// Hash of the preview/screenshot image, if any.
    /// </summary>
    public long? PreviewImageHash { get; init; }

    /// <summary>
    /// Required game version, if any.
    /// </summary>
    public string? GameVersion { get; init; }

    /// <summary>
    /// Required game CRC, if any.
    /// </summary>
    public string? GameCrc { get; init; }

    /// <summary>
    /// Set of engine features required by this addon, if any.
    /// </summary>
    public ImmutableArray<FeatureEnum>? RequiredFeatures { get; init; }

    /// <summary>
    /// Main CON script file name, if any.
    /// </summary>
    public string? MainCon { get; init; }

    /// <summary>
    /// Additional CON script file names, if any.
    /// </summary>
    public ImmutableArray<string>? AddCons { get; init; }

    /// <summary>
    /// Main DEF definition file name, if any.
    /// </summary>
    public string? MainDef { get; init; }

    /// <summary>
    /// Additional DEF definition file names, if any.
    /// </summary>
    public ImmutableArray<string>? AddDefs { get; init; }

    /// <summary>
    /// RTS script file name, if any.
    /// </summary>
    public string? Rts { get; init; }

    /// <summary>
    /// INI configuration file name (Blood), if any.
    /// </summary>
    public string? Ini { get; init; }

    /// <summary>
    /// Main RFF package file name (Blood), if any.
    /// </summary>
    public string? Rff { get; init; }

    /// <summary>
    /// Sound RFF package file name (Blood), if any.
    /// </summary>
    public string? Snd { get; init; }

    /// <summary>
    /// Addon dependencies keyed by id with optional version constraint.
    /// </summary>
    public Dictionary<string, string?>? Dependencies { get; init; }

    /// <summary>
    /// Incompatible addons keyed by id with optional version constraint.
    /// </summary>
    public Dictionary<string, string?>? Incompatibles { get; init; }

    /// <summary>
    /// The starting map definition, if any.
    /// </summary>
    public IStartMap? StartMap { get; init; }

    /// <summary>
    /// Custom executables per OS and port, if any.
    /// </summary>
    public Dictionary<OSEnum, Dictionary<PortEnum, string>>? Executables { get; set; }

    /// <summary>
    /// Launch options per option name, if any.
    /// </summary>
    public Dictionary<string, Dictionary<string, OptionalParameterTypeEnum>>? Options { get; set; }
}
