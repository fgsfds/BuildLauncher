using Common.Common.Helpers;
using Common.Enums;
using System.Text.Json.Serialization;

namespace Common.Serializable.Addon;

public sealed class SupportedGameDto
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
[JsonSerializable(typeof(SupportedGameDto))]
public sealed partial class SupportedGameDtoContext : JsonSerializerContext;
