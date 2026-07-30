using System.Collections.Immutable;
using System.Text;
using Addons.Addons;
using Core.All.Enums;
using Games.Games;

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
        MoveSaveFilesFromStorage(game, campaign);
        FixConfig();
    }


    /// <inheritdoc />
    protected override void GetStartCampaignArgs(StringBuilder sb, BaseGame game, BaseAddon addon)
    {
        //don't search for steam/gog installs
        _ = sb.Append(" -usecwd");

        _ = sb.Append(@$" {CmdArguments.AddDirectory}""{game.GameInstallFolder}""");

        if (addon.MainDef is not null)
        {
            _ = sb.Append($@" {CmdArguments.MainDef}""{addon.MainDef}""");
        }
        else
        {
            //overriding default def so gamename.def files are ignored
            _ = sb.Append($@" {CmdArguments.MainDef}""a""");
        }

        if (addon.AdditionalDefs is not null)
        {
            foreach (var def in addon.AdditionalDefs)
            {
                _ = sb.Append($@" {CmdArguments.AddDef}""{def}""");
            }
        }


        if (game is SlaveGame sGame)
        {
            GetSlaveArgs(sb, sGame, addon);
        }
        else
        {
            throw new NotSupportedException($"Mod type {addon.Type} for game {game} is not supported");
        }
    }
}
