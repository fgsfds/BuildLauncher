using Common.Enums;
using System.Text.Json.Serialization;

namespace Common.Entities
{
    public sealed class AddonsJsonEntity
    {
        [JsonPropertyName("Id")]
        public required string Id { get; set; }

        [JsonPropertyName("AddonType")]
        public required AddonTypeEnum AddonType { get; set; }

        [JsonPropertyName("Game")]
        public required GameEnum Game { get; set; }

        [JsonPropertyName("DownloadUrl")]
        public required string DownloadUrl { get; set; }

        [JsonPropertyName("Title")]
        public required string Title { get; set; }

        [JsonPropertyName("Version")]
        public required string Version { get; set; }

        [JsonPropertyName("FileSize")]
        public required long FileSize { get; set; }

        [JsonPropertyName("Dependencies")]
        public Dictionary<string, string?>? Dependencies { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        [JsonPropertyName("Author")]
        public string? Author { get; set; }
    }


    [JsonSourceGenerationOptions(
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = [
            typeof(JsonStringEnumConverter<GameEnum>),
            typeof(JsonStringEnumConverter<AddonTypeEnum>)
            ]
    )]
    [JsonSerializable(typeof(List<AddonsJsonEntity>))]
    public sealed partial class AddonsJsonEntityListContext : JsonSerializerContext;
}
