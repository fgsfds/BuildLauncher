using System.Text.Json.Serialization;
using Core.All.Interfaces;

namespace Core.All.Serializable.Addon;

public sealed record MapFileJsonModel : IStartMap
{
    [JsonPropertyName("file")]
    public required string File { get; set; }
}


[JsonSourceGenerationOptions(RespectNullableAnnotations = true)]
[JsonSerializable(typeof(MapFileJsonModel))]
public sealed partial class MapFileJsonContext : JsonSerializerContext;
