using Common.Interfaces;
using System.Text.Json.Serialization;

namespace Mods.Serializable.Addon
{
    public class MapSlotDto : IStartMap
    {
        [JsonPropertyName("volume")]
        public required string Episode { get; set; }

        [JsonPropertyName("level")]
        public required string Level { get; set; }
    }

    [JsonSerializable(typeof(MapSlotDto))]
    public sealed partial class MapSlotDtoContext : JsonSerializerContext;
}
