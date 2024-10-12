using Common.Interfaces;
using System.Text.Json.Serialization;

namespace Common.Serializable.Addon;

public sealed class MapSlotDto : IStartMap
{
    [JsonPropertyName("volume")]
    public required int Episode { get; set; }

    [JsonPropertyName("level")]
    public required int Level { get; set; }
}

[JsonSerializable(typeof(MapSlotDto))]
public sealed partial class MapSlotDtoContext : JsonSerializerContext;
