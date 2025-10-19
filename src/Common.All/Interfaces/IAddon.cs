using System.Collections.Immutable;
using Common.All.Enums;

namespace Common.All.Interfaces;

public interface IAddon
{
    /// <summary>
    /// Addon ID
    /// </summary>
    AddonId AddonId { get; init; }

    /// <summary>
    /// Type of the addon
    /// </summary>
    AddonTypeEnum Type { get; init; }

    /// <summary>
    /// List of supported games
    /// </summary>
    GameStruct SupportedGame { get; init; }

    /// <summary>
    /// Features required to run port
    /// </summary>
    ImmutableArray<FeatureEnum>? RequiredFeatures { get; init; }

    /// <summary>
    /// Name of the addon
    /// </summary>
    string Title { get; init; }

    /// <summary>
    /// Addon author
    /// </summary>
    string? Author { get; init; }

    /// <summary>
    /// Addon description
    /// </summary>
    string? Description { get; init; }

    /// <summary>
    /// Path to addon file
    /// </summary>
    string? PathToFile { get; init; }

    /// <summary>
    /// Name of the addon file
    /// </summary>
    string? FileName { get; }

    /// <summary>
    /// Cover image
    /// </summary>
    long? GridImageHash { get; init; }

    /// <summary>
    /// Preview image
    /// </summary>
    long? PreviewImageHash { get; init; }

    /// <summary>
    /// List of addons that the current addon requires to work
    /// </summary>
    IReadOnlyDictionary<string, string?>? DependentAddons { get; init; }

    /// <summary>
    /// List of addons that the current addon is incompatible with
    /// </summary>
    IReadOnlyDictionary<string, string?>? IncompatibleAddons { get; init; }

    /// <summary>
    /// Main def file
    /// </summary>
    string? MainDef { get; init; }

    /// <summary>
    /// Additional def files
    /// </summary>
    ImmutableArray<string>? AdditionalDefs { get; init; }

    /// <summary>
    /// Map that will be started when the addon is loaded
    /// </summary>
    IStartMap? StartMap { get; init; }

    /// <summary>
    /// Map that will be started when the addon is loaded
    /// </summary>
    IReadOnlyDictionary<OSEnum, string>? Executables { get; init; }

    /// <summary>
    /// Is addon unpacked to a folder
    /// </summary>
    bool IsFolder { get; init; }

    /// <summary>
    /// Is the item marked as a favorite.
    /// </summary>
    bool IsFavorite { get; set; }


    /// <summary>
    /// Create markdown description of the addon
    /// </summary>
    string ToMarkdownString();
}