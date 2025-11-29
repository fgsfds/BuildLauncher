using System.Collections.Immutable;
using System.Text;
using Common.All;
using Common.All.Enums;
using Common.All.Interfaces;

namespace Addons.Addons;

/// <summary>
/// Base class for campaigns and maps
/// </summary>
public abstract class BaseAddon
{
    /// <summary>
    /// Addon ID
    /// </summary>
    public required AddonId AddonId { get; init; }

    /// <summary>
    /// Type of the addon
    /// </summary>
    public required AddonTypeEnum Type { get; init; }

    /// <summary>
    /// List of supported games
    /// </summary>
    public required GameStruct SupportedGame { get; init; }

    /// <summary>
    /// Name of the addon
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Addon author
    /// </summary>
    public required string? Author { get; init; }

    /// <summary>
    /// Addon description
    /// </summary>
    public required string? Description { get; init; }

    /// <summary>
    /// Features required to run addon
    /// </summary>
    public required ImmutableArray<FeatureEnum>? RequiredFeatures { get; init; }

    /// <summary>
    /// List of addons that the current addon requires to work
    /// </summary>
    public required IReadOnlyDictionary<string, string?>? DependentAddons { get; init; }

    /// <summary>
    /// List of addons that the current addon is incompatible with
    /// </summary>
    public required IReadOnlyDictionary<string, string?>? IncompatibleAddons { get; init; }

    /// <summary>
    /// Path to addon file
    /// </summary>
    public required string? PathToFile { get; init; }

    /// <summary>
    /// Cover image hash
    /// </summary>
    public required long? GridImageHash { get; init; }

    /// <summary>
    /// Preview image hash
    /// </summary>
    public required long? PreviewImageHash { get; init; }

    /// <summary>
    /// Main def file
    /// </summary>
    public required string? MainDef { get; init; }

    /// <summary>
    /// Additional def files
    /// </summary>
    public required ImmutableArray<string>? AdditionalDefs { get; init; }

    /// <summary>
    /// Map that will be started when the addon is loaded
    /// </summary>
    public required IStartMap? StartMap { get; init; }

    /// <summary>
    /// Is addon unpacked to a folder
    /// </summary>
    public required bool IsUnpacked { get; init; }

    /// <summary>
    /// Is the item marked as a favorite.
    /// </summary>
    public bool IsFavorite { get; set; }

    /// <inheritdoc/>
    public required Dictionary<OSEnum, Dictionary<PortEnum, string>>? Executables { get; init; }

    /// <summary>
    /// Name of the addon file
    /// </summary>
    public string? FileName => PathToFile is null ? null : Path.GetFileName(PathToFile);

    /// <inheritdoc/>
    public override string ToString() => Title;

    /// <summary>
    /// Create markdown description of the addon
    /// </summary>
    public string ToMarkdownString()
    {
        StringBuilder description = new($"## {Title}");

        if (AddonId.Version is not null)
        {
            _ = description.Append($"{Environment.NewLine}{Environment.NewLine}#### v{AddonId.Version}");
        }

        if (Author is not null)
        {
            _ = description.Append($"{Environment.NewLine}{Environment.NewLine}*by {Author}*");
        }

        if (Description is not null)
        {
            var lines = Description.Split("\n");

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("http"))
                {
                    var line = lines[i].Trim();
                    lines[i] = $"[{line}]({line})";
                }
            }

            _ = description.Append(Environment.NewLine + Environment.NewLine).AppendJoin(Environment.NewLine + Environment.NewLine, lines);
        }

        if (DependentAddons is not null && Type is not AddonTypeEnum.Official)
        {
            _ = description.Append($"{Environment.NewLine}{Environment.NewLine}#### Requires:{Environment.NewLine}").AppendJoin(Environment.NewLine + Environment.NewLine, DependentAddons.Keys);
        }

        if (IncompatibleAddons is not null)
        {
            _ = description.Append($"{Environment.NewLine}{Environment.NewLine}#### Incompatible with:{Environment.NewLine}").AppendJoin(Environment.NewLine + Environment.NewLine, IncompatibleAddons.Keys);
        }

        return description.ToString();
    }
}
