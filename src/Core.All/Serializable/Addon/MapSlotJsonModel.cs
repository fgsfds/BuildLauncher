using System.Text.Json.Serialization;
using Core.All.Interfaces;

namespace Core.All.Serializable.Addon;

/// <summary>
///     Represents a start map defined by an episode and level slot.
/// </summary>
public sealed record MapSlotJsonModel : IStartMap
{
    /// <summary>
    ///     Gets or sets the episode number.
    /// </summary>
    [JsonPropertyName("volume")]
    public required int Episode { get; set; }

    /// <summary>
    ///     Gets or sets the level number.
    /// </summary>
    [JsonPropertyName("level")]
    public required int Level { get; set; }
}


/// <summary>
///     Source generation context for <see cref="MapSlotJsonModel" /> serialization.
/// </summary>
[JsonSourceGenerationOptions(RespectNullableAnnotations = true)]
[JsonSerializable(typeof(MapSlotJsonModel))]
public sealed partial class MapSlotJsonContext : JsonSerializerContext;
