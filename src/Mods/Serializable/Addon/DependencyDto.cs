using System.Text.Json.Serialization;

namespace Mods.Serializable
{
    public sealed class DependencyDto
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }
    }

    [JsonSerializable(typeof(DependencyDto))]
    public sealed partial class DependencyContext : JsonSerializerContext;
}
