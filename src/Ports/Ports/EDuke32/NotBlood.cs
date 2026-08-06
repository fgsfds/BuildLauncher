using System.Collections.Immutable;
using Addons.Addons;
using Core.All.Enums;
using Games.Games;
using Ports.Helpers;

namespace Ports.Ports.EDuke32;

/// <summary>
///     NotBlood port.
/// </summary>
public sealed class NotBlood : NBlood
{
    /// <inheritdoc />
    public override PortEnum PortEnum => PortEnum.NotBlood;

    /// <inheritdoc />
    protected override string WinExe => "notblood.exe";

    /// <inheritdoc />
    protected override string LinExe => "notblood";

    /// <inheritdoc />
    public override string Name => "NotBlood";

    /// <inheritdoc />
    public override ImmutableHashSet<GameEnum> SupportedGames { get; } = [GameEnum.Blood];

    /// <inheritdoc />
    public override ImmutableHashSet<FeatureEnum> SupportedFeatures { get; } =
    [
        FeatureEnum.Modern_Types,
        FeatureEnum.Hightile,
        FeatureEnum.Models,
        FeatureEnum.TileFromTexture
    ];

    /// <inheritdoc />
    protected override string ConfigFile => "notblood.cfg";


    /// <inheritdoc />
    public override void BeforeStart(BaseGame game, BaseAddon campaign)
    {
        SaveFilesHelper.MoveSaveFilesFromStorage(
            GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id),
            GetGameSaveFilesFolder(game, campaign));
        FixConfig();
    }
}
