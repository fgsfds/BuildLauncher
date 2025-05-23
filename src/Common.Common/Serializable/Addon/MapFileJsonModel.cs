using System.Text.Json.Serialization;
using Common.Interfaces;

namespace Common.Serializable.Addon;

public sealed class MapFileJsonModel : IStartMap
{
    [JsonPropertyName("file")]
    public required string File { get; set; }
}


[JsonSourceGenerationOptions(RespectNullableAnnotations = true)]
[JsonSerializable(typeof(MapFileJsonModel))]
public sealed partial class MapFileJsonModelContext : JsonSerializerContext;
