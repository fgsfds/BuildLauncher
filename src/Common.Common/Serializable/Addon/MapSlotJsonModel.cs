using System.Text.Json.Serialization;
using Common.Interfaces;

namespace Common.Serializable.Addon;

public sealed class MapSlotJsonModel : IStartMap
{
    [JsonPropertyName("volume")]
    public required int Episode { get; set; }

    [JsonPropertyName("level")]
    public required int Level { get; set; }
}


[JsonSourceGenerationOptions(RespectNullableAnnotations = true)]
[JsonSerializable(typeof(MapSlotJsonModel))]
public sealed partial class MapSlotJsonModelContext : JsonSerializerContext;
