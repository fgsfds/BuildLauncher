using Common.Enums;
using Common.Helpers;
using System.Text;
using System.Text.Json.Serialization;

namespace Mods.Serializable
{
    public sealed class DownloadableMod
    {
        public Guid Guid { get; set; }

        public string DownloadUrl { get; set; }

        public string Name { get; set; }

        public float Version { get; set; }

        public string? Author { get; set; }

        public string? Url { get; init; }

        public GameEnum Game { get; set; }

        public ModTypeEnum ModType { get; set; }

        public string Description { get; set; }

        public long FileSize { get; set; }

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
        public string VersionString => $"v{Version:0.0#}";

        [JsonIgnore]
        public string FileSizeString => FileSize.ToSizeString();


        public string ToMarkdownString()
        {
            StringBuilder description = new($"## {Name}{Environment.NewLine}");

            description.Append($"{Environment.NewLine}#### v{Version:0.0#}");

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

            return description.ToString();
        }
    }


    [JsonSourceGenerationOptions(
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = [
            typeof(JsonStringEnumConverter<PortEnum>),
            typeof(JsonStringEnumConverter<GameEnum>),
            typeof(JsonStringEnumConverter<ModTypeEnum>)
            ]
    )]
    [JsonSerializable(typeof(List<DownloadableMod>))]
    public sealed partial class DownloadableModManifestsListContext : JsonSerializerContext;
}
