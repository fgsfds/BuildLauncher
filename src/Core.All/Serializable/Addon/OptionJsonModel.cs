using System.Text.Json.Serialization;
using Core.All.Enums;

namespace Core.All.Serializable.Addon;

public sealed class OptionJsonModel
{
    [JsonPropertyName("name")]
    public string OptionName { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    public Dictionary<string, OptionalParameterTypeEnum>? Parameters { get; set; }
}
