using Common.Enums;
using System.Text.Json.Serialization;

namespace Mods.Serializable
{
    public sealed class AddonManifest
    {
        [JsonRequired]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("game")]
        [JsonConverter(typeof(SingleOrArrayConverter<GameEnum>))]
        public List<GameEnum> SupportedGames { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("description")]
        [JsonConverter(typeof(StringOrDescriptionConverter))]
        public Description Description { get; set; }

        [JsonPropertyName("preview")]
        public string PreviewImage { get; set; }

        [JsonPropertyName("GRP")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public List<string> Grps { get; set; }

        [JsonPropertyName("CON")]
        [JsonConverter(typeof(SingleOrArrayConverter<Typed>))]
        public List<Typed> Cons { get; set; }

        [JsonPropertyName("DEF")]
        [JsonConverter(typeof(SingleOrArrayConverter<Typed>))]
        public List<Typed> Defs { get; set; }

        [JsonPropertyName("RTS")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public List<string> Rtss { get; set; }

        [JsonPropertyName("gamecrc")]
        [JsonConverter(typeof(SingleOrArrayConverter<int>))]
        public List<int> SupportedGamesCrcs { get; set; }

        [JsonPropertyName("dependencies")]
        [JsonConverter(typeof(SingleOrArrayConverter<Addon>))]
        public List<Addon> Dependencies { get; set; }

        [JsonPropertyName("incompatibles")]
        [JsonConverter(typeof(SingleOrArrayConverter<Addon>))]
        public List<Addon> Incompatibles { get; set; }
    }

    [JsonSerializable(typeof(AddonManifest))]
    public sealed partial class AddonManifestContext : JsonSerializerContext;


    public sealed class DescriptionPath
    {
        [JsonPropertyName("path")]
        public string PathToTxt { get; set; }
    }

    [JsonSerializable(typeof(DescriptionPath))]
    public sealed partial class DescriptionContext : JsonSerializerContext;


    public sealed class Typed
    {
        [JsonPropertyName("type")]
        public ScriptTypeEnum Type { get; set; }

        [JsonPropertyName("path")]
        public string PathToFile { get; set; }
    }

    [JsonSourceGenerationOptions(Converters = [typeof(JsonStringEnumConverter<ScriptTypeEnum>)])]
    [JsonSerializable(typeof(Typed))]
    public sealed partial class TypedContext : JsonSerializerContext;


    public sealed class Addon
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }
    }

    [JsonSerializable(typeof(Addon))]
    public sealed partial class AddonContext : JsonSerializerContext;


    public sealed class Description
    {
        public bool IsText { get; set; }

        public string Text { get; set; }
    }
}
