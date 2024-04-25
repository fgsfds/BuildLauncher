using Common.Enums;
using Common.Interfaces;
using System.Text.Json.Serialization;

namespace Mods.Serializable
{
    public sealed class AddonDto
    {
        [JsonRequired]
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonRequired]
        [JsonPropertyName("type")]
        public required AddonTypeEnum Type { get; set; }

        [JsonPropertyName("game")]
        [JsonConverter(typeof(SingleOrArrayConverter<GameEnum>))]
        public List<GameEnum>? SupportedGames { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("author")]
        public string? Author { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("description")]
        [JsonConverter(typeof(StringOrDescriptionConverter))]
        public object? Description { get; set; }

        [JsonPropertyName("preview")]
        public string? PreviewImage { get; set; }

        [JsonPropertyName("grp")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public List<string>? Grps { get; set; }

        [JsonPropertyName("con")]
        [JsonConverter(typeof(SingleOrArrayConverter<ScriptDto>))]
        public List<ScriptDto>? Cons { get; set; }

        [JsonPropertyName("def")]
        [JsonConverter(typeof(SingleOrArrayConverter<ScriptDto>))]
        public List<ScriptDto>? Defs { get; set; }

        [JsonPropertyName("rts")]
        public string? Rts { get; set; }

        [JsonPropertyName("ini")]
        public string? Ini { get; set; }

        [JsonPropertyName("rff")]
        public string? Rff { get; set; }

        [JsonPropertyName("snd")]
        public string? Snd { get; set; }

        [JsonPropertyName("gamecrc")]
        [JsonConverter(typeof(SingleOrArrayConverter<int>))]
        public List<int>? SupportedGamesCrcs { get; set; }

        [JsonPropertyName("dependencies")]
        [JsonConverter(typeof(SingleOrArrayConverter<DependencyDto>))]
        public List<DependencyDto>? Dependencies { get; set; }

        [JsonPropertyName("incompatibles")]
        [JsonConverter(typeof(SingleOrArrayConverter<DependencyDto>))]
        public List<DependencyDto>? Incompatibles { get; set; }

        [JsonPropertyName("ports")]
        [JsonConverter(typeof(SingleOrArrayConverter<PortEnum>))]
        public List<PortEnum>? SupportedPorts { get; set; }

        [JsonPropertyName("startmap")]
        [JsonConverter(typeof(IStartMapConverter))]
        public IStartMap? StartMap { get; set; }

        [JsonPropertyName("features")]
        [JsonConverter(typeof(SingleOrArrayConverter<FeaturesEnum>))]
        public List<FeaturesEnum>? RequiredFeatures { get; set; }
    }

    [JsonSourceGenerationOptions(
        Converters = [
            typeof(JsonStringEnumConverter<PortEnum>), 
            typeof(JsonStringEnumConverter<GameEnum>),
            typeof(JsonStringEnumConverter<AddonTypeEnum>),
            typeof(JsonStringEnumConverter<FeaturesEnum>)
            ])]
    [JsonSerializable(typeof(AddonDto))]
    public sealed partial class AddonManifestContext : JsonSerializerContext;
}
