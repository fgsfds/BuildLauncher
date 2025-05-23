using System.Text.Json.Serialization;
using Common.Enums;

namespace Common.Serializable.Addon;

public sealed class SupportedGameJsonModel
{
    [JsonPropertyName("name")]
    [JsonConverter(typeof(GameEnumJsonConverter))]
    public required GameEnum Game { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("crc")]
    public string? Crc { get; set; }
}


[JsonSourceGenerationOptions(
    RespectNullableAnnotations = true,
    Converters = [
        typeof(GameEnumJsonConverter)
        ]
    )]
[JsonSerializable(typeof(SupportedGameJsonModel))]
public sealed partial class SupportedGameJsonModelContext : JsonSerializerContext;
