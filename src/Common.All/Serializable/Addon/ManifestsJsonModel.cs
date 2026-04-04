using System.Text.Json.Serialization;
using Common.All.Enums;

namespace Common.All.Serializable.Addon;

[JsonSourceGenerationOptions(
    Converters = [
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
[JsonSerializable(typeof(List<AddonJsonModel>))]
public sealed partial class ManifestsJsonModelContext : JsonSerializerContext;
