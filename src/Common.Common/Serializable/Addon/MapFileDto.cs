using Common.Interfaces;
using System.Text.Json.Serialization;

namespace Common.Serializable.Addon;

public sealed class MapFileDto : IStartMap
{
    [JsonPropertyName("file")]
    public required string File { get; set; }
}


[JsonSourceGenerationOptions(RespectNullableAnnotations = true)]
[JsonSerializable(typeof(MapFileDto))]
public sealed partial class MapFileDtoContext : JsonSerializerContext;
