using Common;
using Common.Enums;
using Common.Interfaces;
using System.Text;

namespace Mods.Addons
{
    /// <summary>
    /// Base class for campaigns and maps
    /// </summary>
    public abstract class Addon : IAddon
    {
        /// <inheritdoc/>
        public required AddonTypeEnum Type { get; init; }

        /// <inheritdoc/>
        public required GameStruct SupportedGame { get; init; }

        /// <inheritdoc/>
        public required HashSet<FeatureEnum>? RequiredFeatures { get; init; }

        /// <inheritdoc/>
        public required string Id { get; init; }

        /// <inheritdoc/>
        public required string Title { get; init; }

        /// <inheritdoc/>
        public required string? Version { get; init; }

        /// <inheritdoc/>
        public required string? Author { get; init; }

        /// <inheritdoc/>
        public required string? Description { get; init; }

        /// <inheritdoc/>
        public required Dictionary<string, string?>? DependentAddons { get; init; }

        /// <inheritdoc/>
        public required Dictionary<string, string?>? IncompatibleAddons { get; init; }

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
        public string? FileName => PathToFile is null ? null : Path.GetFileName(PathToFile);

        public override string ToString() => Title;


        /// <inheritdoc/>
        public string ToMarkdownString()
        {
            StringBuilder description = new($"## {Title}");

            if (Version is not null)
            {
                description.Append($"{Environment.NewLine}{Environment.NewLine}#### v{Version}");
            }

            if (Author is not null)
            {
                description.Append($"{Environment.NewLine}{Environment.NewLine}*by {Author}*");
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

                description.Append(Environment.NewLine + Environment.NewLine).AppendJoin(Environment.NewLine + Environment.NewLine, lines);
            }

            if (DependentAddons is not null && Type is not AddonTypeEnum.Official)
            {
                description.Append($"{Environment.NewLine}{Environment.NewLine}#### Requires:{Environment.NewLine}").AppendJoin(Environment.NewLine + Environment.NewLine, DependentAddons.Keys);
            }

            if (IncompatibleAddons is not null)
            {
                description.Append($"{Environment.NewLine}{Environment.NewLine}#### Incompatible with:{Environment.NewLine}").AppendJoin(Environment.NewLine + Environment.NewLine, IncompatibleAddons.Keys);
            }

            return description.ToString();
        }
    }
}
