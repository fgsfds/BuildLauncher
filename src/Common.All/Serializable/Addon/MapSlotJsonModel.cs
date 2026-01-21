using System.Text.Json.Serialization;
using Common.All.Interfaces;

namespace Common.All.Serializable.Addon;

public sealed class MapSlotJsonModel : IStartMap
{
    [JsonPropertyName("volume")]
    public required int Episode { get; set; }

    [JsonPropertyName("level")]
    public required int Level { get; set; }
}


[JsonSerializable(typeof(MapSlotJsonModel))]
public sealed partial class MapSlotJsonModelContext : JsonSerializerContext;
