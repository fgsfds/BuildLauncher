using Common.Enums;
using System.Text;
using System.Text.Json.Serialization;

namespace Mods.Serializable
{
    public sealed class DownloadableMod
    {
        public Guid Guid { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public GameEnum Game { get; set; }

        public ModTypeEnum ModType { get; set; }

        public float Version { get; set; }

        public string Description { get; set; }

        public string? Author { get; set; }


        public override string ToString() => Name;

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
