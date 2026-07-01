using System.Text.Json.Serialization;
using Core.All.Enums;

namespace Core.All.Serializable.Addon;

/// <summary>
///     Represents a supported game entry for an addon, including optional version and CRC constraints.
/// </summary>
public sealed record SupportedGameJsonModel
{
    /// <summary>
    ///     Gets or sets the game identifier.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonConverter(typeof(GameEnumJsonConverter))]
    public required GameEnum Game { get; set; }

    /// <summary>
    ///     Gets or sets the optional required game version.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    ///     Gets or sets the optional required game CRC hash.
    /// </summary>
    [JsonPropertyName("crc")]
    public string? Crc { get; set; }
}


/// <summary>
///     Source generation context for <see cref="SupportedGameJsonModel" /> serialization.
/// </summary>
[JsonSourceGenerationOptions(
    RespectNullableAnnotations = true,
    Converters =
    [
        typeof(GameEnumJsonConverter)
    ]
    )]
[JsonSerializable(typeof(SupportedGameJsonModel))]
public sealed partial class SupportedGameJsonContext : JsonSerializerContext;
