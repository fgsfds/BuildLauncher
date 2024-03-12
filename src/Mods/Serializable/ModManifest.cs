using Common.Enums;
using System.Text.Json.Serialization;

namespace Mods.Serializable
{
    public sealed class ModManifest
    {
        public required Guid Guid { get; init; }

        public required ModTypeEnum ModType { get; init; }

        public required GameEnum Game { get; init; }

        public required string Name { get; init; }

        public required float Version { get; init; }

        public required string Description { get; init; }


        public string? Url { get; init; } = null;

        public string? Author { get; init; } = null;

        public string? Addon { get; init; } = null;

        public List<string>? SupportedAddons { get; init; } = null;

        public List<PortEnum>? SupportedPorts { get; init; } = null;

        public string? StartupFile { get; init; } = null;
    }


    [JsonSourceGenerationOptions(
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = [
            typeof(JsonStringEnumConverter<PortEnum>),
            typeof(JsonStringEnumConverter<GameEnum>),
            typeof(JsonStringEnumConverter<ModTypeEnum>)
            ]
    )]
    [JsonSerializable(typeof(ModManifest))]
    public sealed partial class ModManifestContext : JsonSerializerContext;
}
