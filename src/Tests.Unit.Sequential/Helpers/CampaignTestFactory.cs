using System.Collections.Immutable;
using Addons.Addons;
using Core.All.Enums;
using Core.All.Enums.Versions;
using Core.Client.Helpers;

namespace Tests.Unit.Helpers;

/// <summary>
///     Factory for creating test campaigns with sensible defaults, avoiding
///     repeated boilerplate of required null members.
/// </summary>
internal static class CampaignTestFactory
{
    /// <summary>
    ///     Creates a <see cref="DukeCampaign" /> with default null metadata.
    /// </summary>
    public static DukeCampaign CreateDukeCampaign(
        string addonId,
        GameEnum game,
        AddonFilePathWrapper? fileInfo,
        AddonTypeEnum type = AddonTypeEnum.TC,
        string version = "1.1",
        IReadOnlyDictionary<string, string?>? dependentAddons = null,
        string? mainCon = null,
        IEnumerable<string>? additionalCons = null,
        string? mainDef = null,
        IEnumerable<string>? additionalDefs = null,
        Dictionary<OSEnum, Dictionary<PortEnum, string>>? executables = null)
    {
        return new DukeCampaign
        {
            AddonId = new(addonId, version),
            Type = type,
            Title = addonId,
            SupportedGame = new(game, DukeVersionEnum.Duke3D_Atomic),
            FileInfo = fileInfo,
            Author = null,
            ReleaseDate = null,
            Description = null,
            RequiredFeatures = null,
            DependentAddons = dependentAddons,
            IncompatibleAddons = null,
            GridImageHash = null,
            PreviewImageHash = null,
            MainDef = mainDef,
            AdditionalDefs = additionalDefs?.ToImmutableArray(),
            MainCon = mainCon,
            AdditionalCons = additionalCons?.ToImmutableArray(),
            RTS = null,
            StartMap = null,
            Executables = executables,
            Options = null
        };
    }

    /// <summary>
    ///     Creates a <see cref="GenericCampaign" /> with default null metadata.
    /// </summary>
    public static GenericCampaign CreateGenericCampaign(
        string addonId,
        GameEnum game,
        AddonFilePathWrapper? fileInfo,
        AddonTypeEnum type = AddonTypeEnum.TC,
        Dictionary<OSEnum, Dictionary<PortEnum, string>>? executables = null)
    {
        return new GenericCampaign
        {
            AddonId = new(addonId, null),
            Type = type,
            Title = addonId,
            SupportedGame = new(game),
            FileInfo = fileInfo,
            Author = null,
            ReleaseDate = null,
            Description = null,
            RequiredFeatures = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            GridImageHash = null,
            PreviewImageHash = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            Executables = executables,
            Options = null
        };
    }
}
