using Common.Enums;
using Common.Interfaces;
using System.Text.Json.Serialization;

namespace Mods.Serializable;

public sealed class AddonDto
{
    [JsonRequired]
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonRequired]
    [JsonPropertyName("type")]
    public required AddonTypeEnum AddonType { get; set; }

    [JsonRequired]
    [JsonPropertyName("game")]
    public required SupportedGameDto SupportedGame { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

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
    public DependencyDto? Dependencies { get; set; }

    [JsonPropertyName("incompatibles")]
    public DependencyDto? Incompatibles { get; set; }

    [JsonPropertyName("startmap")]
    [JsonConverter(typeof(IStartMapConverter))]
    public IStartMap? StartMap { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

[JsonSourceGenerationOptions(
    Converters = [
        typeof(JsonStringEnumConverter<PortEnum>),
        typeof(JsonStringEnumConverter<GameEnum>),
        typeof(JsonStringEnumConverter<AddonTypeEnum>),
        typeof(JsonStringEnumConverter<FeatureEnum>)
        ],
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    AllowTrailingCommas = true,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    )]
[JsonSerializable(typeof(AddonDto))]
public sealed partial class AddonManifestContext : JsonSerializerContext;
