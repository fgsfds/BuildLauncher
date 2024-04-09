using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using System.Text;
using System.Text.Json.Serialization;

namespace Mods.Serializable
{
    public sealed class DownloadableAddonDto : IDownloadableMod
    {
        public string Id { get; set; }

        public string DownloadUrl { get; set; }

        public string Title { get; set; }

        public string? Version { get; set; }

        public string? Author { get; set; }

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
        public string FileSizeString => FileSize.ToSizeString();


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
    [JsonSerializable(typeof(List<DownloadableAddonDto>))]
    public sealed partial class DownloadableModManifestsListContext : JsonSerializerContext;
}
