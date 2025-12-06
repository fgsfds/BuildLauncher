using System.Text.Json.Serialization;
using Common.All.Enums;

namespace Common.All.Serializable.Addon;

public sealed class OptionJsonModel
{
    [JsonPropertyName("name")]
    public string OptionName { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    public Dictionary<string, OptionalParameterTypeEnum>? Parameters { get; set; }
}
