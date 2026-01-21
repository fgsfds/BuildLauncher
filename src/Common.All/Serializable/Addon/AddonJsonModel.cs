using System.Text.Json.Serialization;
using Common.All.Enums;
using Common.All.Interfaces;

namespace Common.All.Serializable.Addon;

public sealed class AddonJsonModel
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("type")]
    public required AddonTypeEnum AddonType { get; set; }

    [JsonPropertyName("game")]
    public required SupportedGameJsonModel SupportedGame { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("version")]
    public required string Version { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("con_main")]
    public string? MainCon { get; set; }

    [JsonPropertyName("con_modules")]
    public List<string>? AdditionalCons { get; set; }

    [JsonPropertyName("def_main")]
    public string? MainDef { get; set; }

    [JsonPropertyName("def_modules")]
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
    [JsonConverter(typeof(ExecutablesConverter))]
    public Dictionary<OSEnum, Dictionary<PortEnum, string>>? Executables { get; set; }

    [JsonPropertyName("options")]
    public List<OptionJsonModel>? Options { get; set; }

    [Obsolete]
    public Dictionary<OSEnum, string>? ExecutablesOld { get; } = null;
}


[JsonSourceGenerationOptions(
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    AllowTrailingCommas = true,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true,
    UseStringEnumConverter = true
    )]
[JsonSerializable(typeof(AddonJsonModel))]
public sealed partial class AddonManifestContext : JsonSerializerContext;
