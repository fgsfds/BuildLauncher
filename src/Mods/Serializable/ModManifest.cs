using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
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

        public List<PortEnum>? SupportedPorts { get; init; } = null;

        public string? StartupFile { get; init; } = null;


        [JsonIgnore]
        public DukeAddonEnum? DukeAddon
        {
            get
            {
                if (Addon is null || Game is not GameEnum.Duke3D)
                {
                    return null;
                }

                var result = Enum.TryParse(typeof(DukeAddonEnum), Addon.ToString(), out var addon);

                if (!result || addon is not DukeAddonEnum dukeAddon)
                {
                    ThrowHelper.Exception($"Error while parsing enum from {Addon}");
                    return null;
                }

                return dukeAddon;
            }
        }

        [JsonIgnore]
        public WangAddonEnum? WangAddon
        {
            get
            {
                if (Addon is null || Game is not GameEnum.Wang)
                {
                    return null;
                }

                var result = Enum.TryParse(typeof(WangAddonEnum), Addon.ToString(), out var addon);

                if (!result || addon is not WangAddonEnum wangAddon)
                {
                    ThrowHelper.Exception($"Error while parsing enum from {Addon}");
                    return null;
                }

                return wangAddon;
            }
        }

        [JsonIgnore]
        public BloodAddonEnum? BloodAddon
        {
            get
            {
                if (Addon is null || Game is not GameEnum.Blood)
                {
                    return null;
                }

                var result = Enum.TryParse(typeof(BloodAddonEnum), Addon.ToString(), out var addon);

                if (!result || addon is not BloodAddonEnum bloodAddon)
                {
                    ThrowHelper.Exception($"Error while parsing enum from {Addon}");
                    return null;
                }

                return bloodAddon;
            }
        }
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
