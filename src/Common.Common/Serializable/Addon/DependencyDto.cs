using Common.Enums;
using System.Text.Json.Serialization;

namespace Common.Serializable.Addon;

public sealed class DependencyDto
{
    [JsonPropertyName("addons")]
    public List<DependantAddonDto>? Addons { get; set; }

    [JsonPropertyName("features")]
    public List<FeatureEnum>? RequiredFeatures { get; set; }
}

[JsonSerializable(typeof(DependencyDto))]
public sealed partial class DependencyDtoContext : JsonSerializerContext;


public sealed class DependantAddonDto
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }
}

[JsonSerializable(typeof(DependantAddonDto))]
public sealed partial class DependantAddonDtoContext : JsonSerializerContext;
