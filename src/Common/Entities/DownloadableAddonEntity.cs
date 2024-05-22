using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using System.Text;
using System.Text.Json.Serialization;

namespace Common.Entities
{
    public sealed class DownloadableAddonEntity : IDownloadableAddon
    {
        [JsonPropertyName("Id")]
        public required string Id { get; set; }

        [JsonPropertyName("AddonType")]
        public required AddonTypeEnum AddonType { get; set; }

        [JsonPropertyName("Game")]
        public required GameEnum Game { get; set; }

        [JsonPropertyName("DownloadUrl")]
        public required Uri DownloadUrl { get; set; }

        [JsonPropertyName("Title")]
        public required string Title { get; set; }

        [JsonPropertyName("Version")]
        public required string Version { get; set; }

        [JsonPropertyName("FileSize")]
        public required long FileSize { get; set; }

        [JsonPropertyName("IsDisabled")]
        public required bool IsDisabled { get; set; }

        [JsonPropertyName("Installs")]
        public required int Installs { get; set; }

        [JsonPropertyName("Score")]
        public required int Score { get; set; }

        [JsonPropertyName("Dependencies")]
        public List<string>? Dependencies { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        [JsonPropertyName("Author")]
        public string? Author { get; set; }


        [JsonIgnore]
        public bool IsInstalled { get; set; }

        [JsonIgnore]
        public bool HasNewerVersion { get; set; }

        [JsonIgnore]
        public string Status
        {
            get
            {
                if (HasNewerVersion)
                {
                    return "Update available";
                }
                if (IsInstalled)
                {
                    return "Installed";
                }

                return string.Empty;
            }
        }

        [JsonIgnore]
        public string FileSizeString => FileSize.ToSizeString();


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
                description.Append("\n\n").Append("Requires: ").AppendJoin(", ", Dependencies);
            }

            return description.ToString();
        }
    }


    [JsonSourceGenerationOptions(
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = [
            typeof(JsonStringEnumConverter<GameEnum>),
            typeof(JsonStringEnumConverter<AddonTypeEnum>)
            ]
    )]
    [JsonSerializable(typeof(List<DownloadableAddonEntity>))]
    public sealed partial class DownloadableAddonEntityListContext : JsonSerializerContext;
}
