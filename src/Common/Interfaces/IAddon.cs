using Common.Enums;

namespace Common.Interfaces;

public interface IAddon
{
    /// <summary>
    /// Addon ID
    /// </summary>
    string Id { get; init; }

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
    HashSet<FeatureEnum>? RequiredFeatures { get; init; }

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
    Stream? GridImage { get; init; }

    /// <summary>
    /// Preview image
    /// </summary>
    Stream? PreviewImage { get; init; }

    /// <summary>
    /// Addon version
    /// </summary>
    string? Version { get; init; }

    /// <summary>
    /// List of addons that the current addon requires to work
    /// </summary>
    Dictionary<string, string?>? DependentAddons { get; init; }

    /// <summary>
    /// List of addons that the current addon is incompatible with
    /// </summary>
    Dictionary<string, string?>? IncompatibleAddons { get; init; }

    /// <summary>
    /// Main def file
    /// </summary>
    string? MainDef { get; init; }

    /// <summary>
    /// Additional def files
    /// </summary>
    HashSet<string>? AdditionalDefs { get; init; }

    /// <summary>
    /// Map that will be started when the addon is loaded
    /// </summary>
    IStartMap? StartMap { get; init; }

    /// <summary>
    /// Is addon unpacked to a folder
    /// </summary>
    bool IsUnpacked { get; init; }

    /// <summary>
    /// Addon title with text wraping
    /// </summary>
    string TitleWithNewLines { get; }


    /// <summary>
    /// Create markdown description of the addon
    /// </summary>
    string ToMarkdownString();
}