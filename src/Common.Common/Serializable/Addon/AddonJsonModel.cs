using Common.Enums;
using Common.Interfaces;
using System.Text.Json.Serialization;

namespace Common.Serializable.Addon;

public sealed class AddonJsonModel
{
    [JsonRequired]
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonRequired]
    [JsonPropertyName("type")]
    public required AddonTypeEnum AddonType { get; set; }

    [JsonRequired]
    [JsonPropertyName("game")]
    public required SupportedGameJsonModel SupportedGame { get; set; }

    [JsonRequired]
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonRequired]
    [JsonPropertyName("version")]
    public required string Version { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("con_main")]
    public string? MainCon { get; set; }

    [JsonPropertyName("con_modules")]
    //[JsonConverter(typeof(SingleOrArrayConverter<string>))]
    public List<string>? AdditionalCons { get; set; }

    [JsonPropertyName("def_main")]
    public string? MainDef { get; set; }

    [JsonPropertyName("def_modules")]
    //[JsonConverter(typeof(SingleOrArrayConverter<string>))]
    public List<string>? AdditionalDefs { get; set; }

    [JsonPropertyName("rts")]
    public string? Rts { get; set; }

    [JsonPropertyName("ini")]
    public string? Ini { get; set; }

    [JsonPropertyName("rff_main")]
    public string? MainRff { get; set; }

    [JsonPropertyName("rff_sound")]
    public string? SoundRff { get; set; }

    [JsonPropertyName("dependencies")]
    public DependencyJsonModel? Dependencies { get; set; }

    [JsonPropertyName("incompatibles")]
    public DependencyJsonModel? Incompatibles { get; set; }

    [JsonPropertyName("startmap")]
    [JsonConverter(typeof(IStartMapConverter))]
    public IStartMap? StartMap { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("executables")]
    public Dictionary<OSEnum, string>? Executables { get; set; }
}

[JsonSourceGenerationOptions(
    Converters = [
        typeof(JsonStringEnumConverter<PortEnum>),
        //typeof(JsonStringEnumConverter<GameEnum>),
        typeof(GameEnumJsonConverter),
        typeof(JsonStringEnumConverter<AddonTypeEnum>),
        typeof(JsonStringEnumConverter<OSEnum>),
        typeof(JsonStringEnumConverter<FeatureEnum>)
        ],
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    AllowTrailingCommas = true,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    RespectNullableAnnotations = true
    )]
[JsonSerializable(typeof(AddonJsonModel))]
public sealed partial class AddonManifestContext : JsonSerializerContext;
