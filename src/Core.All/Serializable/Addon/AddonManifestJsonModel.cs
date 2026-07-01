using System.Text.Json.Serialization;
using Core.All.Enums;
using Core.All.Interfaces;

namespace Core.All.Serializable.Addon;

/// <summary>
///     Represents an addon manifest as deserialized from addon.json files.
/// </summary>
public sealed record AddonManifestJsonModel
{
    /// <summary>
    ///     Gets or sets the addon identifier.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    ///     Gets or sets the addon type.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("type")]
    public required AddonTypeEnum AddonType { get; set; }

    /// <summary>
    ///     Gets or sets the supported game information.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("game")]
    public required SupportedGameJsonModel SupportedGame { get; set; }

    /// <summary>
    ///     Gets or sets the addon title.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    /// <summary>
    ///     Gets or sets the addon version.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("version")]
    public required string Version { get; set; }

    /// <summary>
    ///     Gets or sets the addon author.
    /// </summary>
    [JsonPropertyName("author")]
    public string? Author { get; set; }

    /// <summary>
    ///     Gets or sets the release date.
    /// </summary>
    [JsonPropertyName("release_date")]
    public DateOnly? ReleaseDate { get; set; }

    /// <summary>
    ///     Gets or sets the main CON file.
    /// </summary>
    [JsonPropertyName("con_main")]
    public string? MainCon { get; set; }

    /// <summary>
    ///     Gets or sets additional CON modules.
    /// </summary>
    [JsonPropertyName("con_modules")]
    public List<string>? AdditionalCons { get; set; }

    /// <summary>
    ///     Gets or sets the main DEF file.
    /// </summary>
    [JsonPropertyName("def_main")]
    public string? MainDef { get; set; }

    /// <summary>
    ///     Gets or sets additional DEF modules.
    /// </summary>
    [JsonPropertyName("def_modules")]
    public List<string>? AdditionalDefs { get; set; }

    /// <summary>
    ///     Gets or sets the RTS file.
    /// </summary>
    [JsonPropertyName("rts")]
    public string? Rts { get; set; }

    /// <summary>
    ///     Gets or sets the INI file.
    /// </summary>
    [JsonPropertyName("ini")]
    public string? Ini { get; set; }

    /// <summary>
    ///     Gets or sets the main RFF file.
    /// </summary>
    [JsonPropertyName("rff_main")]
    public string? MainRff { get; set; }

    /// <summary>
    ///     Gets or sets the sound RFF file.
    /// </summary>
    [JsonPropertyName("rff_sound")]
    public string? SoundRff { get; set; }

    /// <summary>
    ///     Gets or sets the addon dependencies.
    /// </summary>
    [JsonPropertyName("dependencies")]
    public DependencyJsonModel? Dependencies { get; set; }

    /// <summary>
    ///     Gets or sets the incompatible addons.
    /// </summary>
    [JsonPropertyName("incompatibles")]
    public DependencyJsonModel? Incompatibles { get; set; }

    /// <summary>
    ///     Gets or sets the start map definition.
    /// </summary>
    [JsonPropertyName("startmap")]
    [JsonConverter(typeof(IStartMapConverter))]
    public IStartMap? StartMap { get; set; }

    /// <summary>
    ///     Gets or sets the addon description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the per-port executables mapping.
    /// </summary>
    [JsonPropertyName("executables")]
    [JsonConverter(typeof(ExecutablesConverter))]
    public Dictionary<OSEnum, Dictionary<PortEnum, string>>? Executables { get; set; }

    /// <summary>
    ///     Gets or sets the addon options.
    /// </summary>
    [JsonPropertyName("options")]
    public List<OptionJsonModel>? Options { get; set; }

    /// <summary>
    ///     Gets the legacy single-port executables mapping.
    /// </summary>
    [Obsolete]
    public Dictionary<OSEnum, string>? ExecutablesOld { get; }

    /// <summary>
    ///     Gets the addon identifier combining the ID and version.
    /// </summary>
    [JsonIgnore]
    public AddonId AddonId => new(Id, Version);
}


/// <summary>
///     Source generation context for <see cref="AddonManifestJsonModel" /> serialization.
/// </summary>
[JsonSourceGenerationOptions(
    Converters =
    [
        typeof(JsonStringEnumConverter<AddonTypeEnum>),
        typeof(JsonStringEnumConverter<OSEnum>),
        typeof(JsonStringEnumConverter<PortEnum>),
        typeof(JsonStringEnumConverter<FeatureEnum>),
        typeof(JsonStringEnumConverter<OptionalParameterTypeEnum>)
    ],
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    AllowTrailingCommas = true,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    RespectNullableAnnotations = true
    )]
[JsonSerializable(typeof(AddonManifestJsonModel))]
[JsonSerializable(typeof(List<AddonManifestJsonModel>))]
public sealed partial class AddonManifestJsonContext : JsonSerializerContext;
