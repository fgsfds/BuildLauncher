using Common.Enums;
using System.Text.Json.Serialization;

namespace Common.Serializable.Addon;

public sealed class DependencyJsonModel
{
    [JsonPropertyName("addons")]
    public List<DependantAddonJsonModel>? Addons { get; set; }

    [JsonPropertyName("features")]
    public List<FeatureEnum>? RequiredFeatures { get; set; }
}


[JsonSourceGenerationOptions(RespectNullableAnnotations = true)]
[JsonSerializable(typeof(DependencyJsonModel))]
public sealed partial class DependencyDtoContext : JsonSerializerContext;


public sealed class DependantAddonJsonModel
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }
}


[JsonSourceGenerationOptions(RespectNullableAnnotations = true)]
[JsonSerializable(typeof(DependantAddonJsonModel))]
public sealed partial class DependantAddonJsonModelContext : JsonSerializerContext;
