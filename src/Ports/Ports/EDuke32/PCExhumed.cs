using System.Collections.Immutable;
using Addons.Addons;
using Core.All.Enums;
using Games.Games;
using Ports.Builders;
using Ports.Helpers;

namespace Ports.Ports.EDuke32;

/// <summary>
///     PCExhumed port.
/// </summary>
public sealed class PCExhumed : EDuke32
{
    /// <inheritdoc />
    public override PortEnum PortEnum => PortEnum.PCExhumed;

    /// <inheritdoc />
    protected override string WinExe => "pcexhumed.exe";

    /// <inheritdoc />
    protected override string LinExe => throw new NotSupportedException();

    /// <inheritdoc />
    public override string Name => "PCExhumed";

    /// <inheritdoc />
    public override ImmutableHashSet<GameEnum> SupportedGames { get; } = [GameEnum.Slave];

    /// <inheritdoc />
    public override ImmutableHashSet<FeatureEnum> SupportedFeatures { get; } = [FeatureEnum.TileFromTexture];

    /// <inheritdoc />
    protected override string ConfigFile => "pcexhumed.cfg";


    /// <inheritdoc />
    public override void BeforeStart(BaseGame game, BaseAddon campaign)
    {
        SaveFilesHelper.MoveSaveFilesFromStorage(
            GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id),
            GetGameSaveFilesFolder(game, campaign));
        FixConfig();
    }


    /// <inheritdoc />
    protected override void CustomModifyArgs(CmdParametersBuilder sb, BaseGame game, BaseAddon addon)
    {
        _ = sb.AppendSkipSteam();
    }
}
