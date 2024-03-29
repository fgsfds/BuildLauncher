﻿using Common.Enums;
using Common.Interfaces;
using System.Text;

namespace Mods.Mods
{
    /// <summary>
    /// Base class for campaigns and maps
    /// </summary>
    public abstract class BaseMod : IMod
    {
        /// <inheritdoc/>
        public required Guid Guid { get; init; }

        /// <inheritdoc/>
        public required ModTypeEnum ModType { get; init; }

        /// <inheritdoc/>
        public required string DisplayName { get; init; }

        /// <inheritdoc/>
        public required List<PortEnum>? SupportedPorts { get; init; }

        /// <inheritdoc/>
        public required string? Description { get; init; }

        /// <inheritdoc/>
        public required string? PathToFile { get; init; }

        /// <inheritdoc/>
        public required virtual string? StartupFile { get; init; }

        /// <inheritdoc/>
        public string? FileName => PathToFile is null ? null : Path.GetFileName(PathToFile);

        /// <inheritdoc/>
        public required Stream? Image { get; init; }

        /// <inheritdoc/>
        public required float? Version { get; init; }

        /// <inheritdoc/>
        public required string? Url { get; init; }

        /// <inheritdoc/>
        public required string? Author { get; init; }

        /// <inheritdoc/>
        public required bool IsOfficial { get; init; }

        /// <inheritdoc/>
        public string? Addon { get; protected set; }

        /// <inheritdoc/>
        public required bool IsLoose { get; init; }

        /// <inheritdoc/>
        public string? DefFileContents { get; init; }


        public override string ToString() => DisplayName;

        /// <inheritdoc/>
        public string ToMarkdownString()
        {
            StringBuilder description = new($"## {DisplayName}{Environment.NewLine}");

            if (Version is not null)
            {
                description.Append($"{Environment.NewLine}#### v{(float)Version:0.0#}");
            }

            if (Author is not null)
            {
                description.Append($"{Environment.NewLine}{Environment.NewLine}*by {Author}*");
            }

            if (Url is not null)
            {
                description.Append($"{Environment.NewLine}{Environment.NewLine}[{Url}]({Url})");
            }

            if (Description is not null)
            {
                description.Append(Environment.NewLine + Environment.NewLine + Description);
            }

            if (SupportedPorts is not null)
            {
                description.Append(Environment.NewLine + Environment.NewLine + $"Only supported by: *{string.Join(", ", SupportedPorts)}*");
            }

            return description.ToString();
        }
    }
}
