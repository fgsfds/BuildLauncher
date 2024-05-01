using Common.Enums;
using System.Text.Json.Serialization;

namespace Mods.Serializable
{
    public sealed class ScriptDto
    {
        [JsonPropertyName("type")]
        public required ScriptTypeEnum Type { get; set; }

        [JsonPropertyName("path")]
        public required string PathToFile { get; set; }
    }

    [JsonSourceGenerationOptions(Converters = [typeof(JsonStringEnumConverter<ScriptTypeEnum>)])]
    [JsonSerializable(typeof(ScriptDto))]
    public sealed partial class ScriptContext : JsonSerializerContext;
}
