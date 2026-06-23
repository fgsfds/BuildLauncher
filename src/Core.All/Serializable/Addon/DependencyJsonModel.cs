using System.Text.Json.Serialization;
using Core.All.Enums;

namespace Core.All.Serializable.Addon;

public sealed record DependencyJsonModel
{
    [JsonPropertyName("addons")]
    public List<DependantAddonJsonModel>? Addons { get; set; }

    [JsonPropertyName("features")]
    public List<FeatureEnum>? RequiredFeatures { get; set; }
}


public sealed record DependantAddonJsonModel
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }
}
