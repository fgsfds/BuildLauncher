using System.Text.Json.Serialization;
using Common.All.Enums;

namespace Common.All.Serializable.Addon;

public sealed class DependencyJsonModel
{
    [JsonPropertyName("addons")]
    public List<DependantAddonJsonModel>? Addons { get; set; }

    [JsonPropertyName("features")]
    public List<FeatureEnum>? RequiredFeatures { get; set; }
}


public sealed class DependantAddonJsonModel
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }
}
