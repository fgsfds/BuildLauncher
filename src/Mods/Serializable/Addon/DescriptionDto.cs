using System.Text.Json.Serialization;

namespace Mods.Serializable
{
    public sealed class DescriptionDto
    {
        [JsonPropertyName("path")]
        public required string PathToTxt { get; set; }
    }

    [JsonSerializable(typeof(DescriptionDto))]
    public sealed partial class DescriptionContext : JsonSerializerContext;
}
