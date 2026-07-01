using System.Text.Json.Serialization;
using Core.All.Interfaces;

namespace Core.All.Serializable.Addon;

/// <summary>
///     Represents a start map defined by a file path.
/// </summary>
public sealed record MapFileJsonModel : IStartMap
{
    /// <summary>
    ///     Gets or sets the map file path.
    /// </summary>
    [JsonPropertyName("file")]
    public required string File { get; set; }
}


/// <summary>
///     Source generation context for <see cref="MapFileJsonModel" /> serialization.
/// </summary>
[JsonSourceGenerationOptions(RespectNullableAnnotations = true)]
[JsonSerializable(typeof(MapFileJsonModel))]
public sealed partial class MapFileJsonContext : JsonSerializerContext;
