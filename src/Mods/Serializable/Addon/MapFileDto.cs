using Common.Interfaces;
using System.Text.Json.Serialization;

namespace Mods.Serializable.Addon;

public sealed class MapFileDto : IStartMap
{
    [JsonPropertyName("file")]
    public required string File { get; set; }
}

[JsonSerializable(typeof(MapFileDto))]
public sealed partial class MapFileDtoContext : JsonSerializerContext;
