using System.Text.Json.Serialization;
using Core.All.Enums;

namespace Core.All.Serializable.Addon;

/// <summary>
///     Represents a set of dependencies or incompatibilities for an addon.
/// </summary>
public sealed record DependencyJsonModel
{
    /// <summary>
    ///     Gets or sets the list of dependent addons.
    /// </summary>
    [JsonPropertyName("addons")]
    public List<DependantAddonJsonModel>? Addons { get; set; }

    /// <summary>
    ///     Gets or sets the required engine features.
    /// </summary>
    [JsonPropertyName("features")]
    public List<FeatureEnum>? RequiredFeatures { get; set; }
}


/// <summary>
///     Represents a dependency on another addon.
/// </summary>
public sealed record DependantAddonJsonModel
{
    /// <summary>
    ///     Gets or sets the dependent addon identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    ///     Gets or sets the required version of the dependent addon.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }
}
