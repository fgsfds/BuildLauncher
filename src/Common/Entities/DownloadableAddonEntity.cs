using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using System.Text;
using System.Text.Json.Serialization;

namespace Common
{
    public sealed class DownloadableAddonEntity : IDownloadableAddon
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonPropertyName("DownloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonPropertyName("Title")]
        public string Title { get; set; }

        [JsonPropertyName("Version")]
        public string Version { get; set; }

        [JsonPropertyName("Author")]
        public string? Author { get; set; }

        [JsonPropertyName("Game")]
        public GameEnum Game { get; set; }

        [JsonPropertyName("AddonType")]
        public AddonTypeEnum AddonType { get; set; }

        [JsonPropertyName("Description")]
        public string Description { get; set; }

        [JsonPropertyName("FileSize")]
        public long FileSize { get; set; }

        [JsonPropertyName("IsDisabled")]
        public bool IsDisabled { get; set; }

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

            return description.ToString();
        }
    }


    [JsonSourceGenerationOptions(
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = [
            typeof(JsonStringEnumConverter<PortEnum>),
            typeof(JsonStringEnumConverter<GameEnum>),
            typeof(JsonStringEnumConverter<AddonTypeEnum>)
            ]
    )]
    [JsonSerializable(typeof(List<DownloadableAddonEntity>))]
    public sealed partial class DownloadableAddonEntityListContext : JsonSerializerContext;
}
