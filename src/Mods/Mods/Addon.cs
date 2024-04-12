using Common.Enums;
using Common.Interfaces;
using System.Text;

namespace Mods.Mods
{
    /// <summary>
    /// Base class for campaigns and maps
    /// </summary>
    public class Addon : IAddon
    {
        /// <inheritdoc/>
        public required AddonTypeEnum Type { get; init; }

        /// <inheritdoc/>
        public required string Id { get; init; }

        /// <inheritdoc/>
        public required string Title { get; init; }

        /// <inheritdoc/>
        public required string? Version { get; init; }

        /// <inheritdoc/>
        public required HashSet<GameEnum>? SupportedGames { get; init; }

        /// <inheritdoc/>
        public required HashSet<int>? SupportedGamesCrcs { get; init; }

        /// <inheritdoc/>
        public required string? Author { get; init; }

        /// <inheritdoc/>
        public required string? Description { get; init; }

        /// <inheritdoc/>
        public required HashSet<PortEnum>? SupportedPorts { get; init; }

        /// <inheritdoc/>
        public required Dictionary<string, string?>? Dependencies { get; init; }

        /// <inheritdoc/>
        public required Dictionary<string, string?>? Incompatibles { get; init; }

        /// <inheritdoc/>
        public required string? PathToFile { get; init; }

        /// <inheritdoc/>
        public required Stream? Image { get; init; }

        /// <inheritdoc/>
        public bool IsAvailable { get; set; }

        /// <inheritdoc/>
        public required string? MainDef { get; init; }

        /// <inheritdoc/>
        public required HashSet<string>? AdditionalDefs { get; init; }

        /// <inheritdoc/>
        public required IStartMap? StartMap { get; init; }

        /// <inheritdoc/>
        public string? FileName => PathToFile is null ? null : Path.GetFileName(PathToFile);

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
                var lines = Description.Split("\r\n");

                for (var i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("http"))
                    {
                        lines[i] = $"[{lines[i]}]({lines[i]})";
                    }
                }

                description.Append(Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine + Environment.NewLine, lines));
            }

            if (SupportedPorts is not null)
            {
                description.Append(Environment.NewLine + Environment.NewLine + $"Only supported by: *{string.Join(", ", SupportedPorts)}*");
            }

            return description.ToString();
        }
    }
}
