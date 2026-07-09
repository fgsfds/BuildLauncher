using System.Text.Json.Serialization;
using Core.All.Enums;

namespace Core.All.Serializable.Addon;

/// <summary>
///     Represents an optional addon parameter configuration.
/// </summary>
public sealed record OptionJsonModel
{
    /// <summary>
    ///     Gets or sets the option name.
    /// </summary>
    [JsonPropertyName("name")]
    public string OptionName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the option parameters mapped by file name.
    /// </summary>
    [JsonPropertyName("parameters")]
    public Dictionary<string, OptionalParameterTypeEnum>? Parameters { get; set; }
}
