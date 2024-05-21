using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using System.Text;

namespace Mods.Addons
{
    /// <summary>
    /// Base class for campaigns and maps
    /// </summary>
    public class Addon : IAddon
    {
        /// <inheritdoc/>
        public required AddonTypeEnum Type { get; init; }

        /// <inheritdoc/>
        public required HashSet<FeatureEnum>? RequiredFeatures { get; init; }

        /// <inheritdoc/>
        public required string Id { get; init; }

        /// <inheritdoc/>
        public required string Title { get; init; }

        /// <inheritdoc/>
        public required string? Version { get; init; }

        /// <inheritdoc/>
        public required GameEnum SupportedGame { get; init; }

        /// <inheritdoc/>
        public required HashSet<PortEnum>? SupportedPorts { get; init; }

        /// <inheritdoc/>
        public required string? Author { get; init; }

        /// <inheritdoc/>
        public required string? Description { get; init; }

        /// <inheritdoc/>
        public required Dictionary<string, string?>? Dependencies { get; init; }

        /// <inheritdoc/>
        public required Dictionary<string, string?>? Incompatibles { get; init; }

        /// <inheritdoc/>
        public required string? PathToFile { get; init; }

        /// <inheritdoc/>
        public required Stream? GridImage { get; init; }

        /// <inheritdoc/>
        public required Stream? PreviewImage { get; init; }

        /// <inheritdoc/>
        public required string? MainDef { get; init; }

        /// <inheritdoc/>
        public required HashSet<string>? AdditionalDefs { get; init; }

        /// <inheritdoc/>
        public required IStartMap? StartMap { get; init; }

        /// <inheritdoc/>
        public required TimeSpan Playtime { get; set; }

        /// <inheritdoc/>
        public string? FileName => PathToFile is null ? null : Path.GetFileName(PathToFile);

        public override string ToString() => Title;


        /// <inheritdoc/>
        public string ToMarkdownString()
        {
            StringBuilder description = new($"## {Title}");

            if (Version is not null)
            {
                description.Append($"\n\n#### v{Version}");
            }

            if (Author is not null)
            {
                description.Append($"\n\n*by {Author}*");
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

                description.Append("\n\n").AppendJoin("\n\n", lines);
            }

            if (Dependencies is not null)
            {
                description.Append("\n\n").Append($"Requires: *{string.Join(", ", Dependencies.Keys)}*");
            }

            if (Incompatibles is not null)
            {
                description.Append("\n\n").Append($"Incompatible with: *{string.Join(", ", Incompatibles.Keys)}*");
            }

            description.Append("\n\n").Append($"#### Play time: *{Playtime.ToTimeString()}*");

            return description.ToString();
        }

        /// <inheritdoc/>
        public void UpdatePlaytime(TimeSpan time) => Playtime = Playtime.Add(time);
    }
}
