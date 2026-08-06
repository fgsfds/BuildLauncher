using System.Collections.Immutable;
using Addons.Addons;
using Core.All.Enums;
using Games.Games;
using Ports.Builders;
using Ports.Helpers;

namespace Ports.Ports.EDuke32;

/// <summary>
///     NBlood port.
/// </summary>
public class NBlood : EDuke32
{
    /// <inheritdoc />
    public override PortEnum PortEnum => PortEnum.NBlood;

    /// <inheritdoc />
    protected override string WinExe => "nblood.exe";

    /// <inheritdoc />
    protected override string LinExe => throw new NotSupportedException();

    /// <inheritdoc />
    public override string Name => "NBlood";

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
    protected override string ConfigFile => "nblood.cfg";

    /// <inheritdoc />
    public override PortCmdArguments CmdArguments => base.CmdArguments with
    {
        SkillLevel = "-s ",
        AddRff = "-rff ",
        AddSnd = "-snd "
    };


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
