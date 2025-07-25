﻿using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Common;
using Common.Enums;
using Common.Interfaces;

namespace Addons.Addons;

/// <summary>
/// Base class for campaigns and maps
/// </summary>
public abstract class BaseAddonEntity : IAddon
{
    /// <inheritdoc/>
    public required AddonId AddonId { get; init; }

    /// <inheritdoc/>
    public required AddonTypeEnum Type { get; init; }

    /// <inheritdoc/>
    public required GameStruct SupportedGame { get; init; }

    /// <inheritdoc/>
    public required ImmutableArray<FeatureEnum>? RequiredFeatures { get; init; }

    /// <inheritdoc/>
    public required string Title { get; init; }

    /// <inheritdoc/>
    public required string? Author { get; init; }

    /// <inheritdoc/>
    public required string? Description { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyDictionary<string, string?>? DependentAddons { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyDictionary<string, string?>? IncompatibleAddons { get; init; }

    /// <inheritdoc/>
    public required string? PathToFile { get; init; }

    /// <inheritdoc/>
    public required long? GridImageHash { get; init; }

    /// <inheritdoc/>
    public required long? PreviewImageHash { get; init; }

    /// <inheritdoc/>
    public required string? MainDef { get; init; }

    /// <inheritdoc/>
    public required ImmutableArray<string>? AdditionalDefs { get; init; }

    /// <inheritdoc/>
    public required IStartMap? StartMap { get; init; }

    /// <inheritdoc/>
    public required bool IsFolder { get; init; }

    /// <inheritdoc/>
    public bool IsFavorite { get; set; }

    /// <inheritdoc/>
    public required IReadOnlyDictionary<OSEnum, string>? Executables { get; init; }

    /// <inheritdoc/>
    public string? FileName => PathToFile is null ? null : Path.GetFileName(PathToFile);

    public override string ToString() => Title;


    /// <inheritdoc/>
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
