using Common.Enums;
using Common.Interfaces;
using System.Text;

namespace Mods.Mods
{
    /// <summary>
    /// Base class for campaigns and maps
    /// </summary>
    public abstract class BaseAddon : IAddon
    {
        /// <inheritdoc/>
        public required string Id { get; init; }

        public GameEnum Game { get; init; }

        /// <inheritdoc/>
        public required ModTypeEnum ModType { get; init; }

        /// <inheritdoc/>
        public required string Title { get; init; }

        /// <inheritdoc/>
        public required List<PortEnum>? SupportedPorts { get; init; }

        /// <inheritdoc/>
        public required string? Description { get; init; }

        /// <inheritdoc/>
        public required string? PathToFile { get; init; }

        /// <inheritdoc/>
        public string? FileName => PathToFile is null ? null : Path.GetFileName(PathToFile);

        /// <inheritdoc/>
        public required Stream? Image { get; init; }

        /// <inheritdoc/>
        public required string? Version { get; init; }

        /// <inheritdoc/>
        public required string? Author { get; init; }

        /// <inheritdoc/>
        public string? Addon { get; protected set; }

        /// <inheritdoc/>
        public required bool IsLoose { get; init; }

        /// <inheritdoc/>
        public string? DefFileContents { get; init; }

        public List<int>? GameCrcs { get; init; }

        public HashSet<string>? Dependencies { get; init; }

        public HashSet<string>? Incompatibles { get; init; }

        public override string ToString() => Title;

        /// <inheritdoc/>
        public string ToMarkdownString()
        {
            StringBuilder description = new($"## {Title}{Environment.NewLine}");

            if (Version is not null)
            {
                description.Append($"{Environment.NewLine}#### v{Version}");
            }

            if (Author is not null)
            {
                description.Append($"{Environment.NewLine}{Environment.NewLine}*by {Author}*");
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
