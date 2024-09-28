using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using System.Text;

namespace Ports.Ports.EDuke32;

/// <summary>
/// PCExhumed port
/// </summary>
public sealed class PCExhumed : EDuke32
{
    /// <inheritdoc/>
    public override PortEnum PortEnum => PortEnum.PCExhumed;

    /// <inheritdoc/>
    public override string Exe => "pcexhumed.exe";

    /// <inheritdoc/>
    public override string Name => "PCExhumed";

    /// <inheritdoc/>
    public override List<GameEnum> SupportedGames => [GameEnum.Exhumed];

    /// <inheritdoc/>
    public override List<FeatureEnum> SupportedFeatures => [];

    /// <inheritdoc/>
    protected override string ConfigFile => "pcexhumed.cfg";


    /// <inheritdoc/>
    protected override void BeforeStart(IGame game, IAddon campaign)
    {
        MoveSaveFiles(game, campaign);
    }


    /// <inheritdoc/>
    protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon)
    {
        //don't search for steam/gog installs
        sb.Append(" -usecwd");

        sb.Append(@$" {AddDirectoryParam}""{game.GameInstallFolder}""");

        if (addon.MainDef is not null)
        {
            sb.Append($@" {MainDefParam}""{addon.MainDef}""");
        }
        else
        {
            //overriding default def so gamename.def files are ignored
            sb.Append($@" {MainDefParam}""a""");
        }

        if (addon.AdditionalDefs is not null)
        {
            foreach (var def in addon.AdditionalDefs)
            {
                sb.Append($@" {AddDefParam}""{def}""");
            }
        }


        if (game is SlaveGame sGame)
        {
            GetSlaveArgs(sb, sGame, addon);
        }
        else
        {
            ThrowHelper.NotImplementedException($"Mod type {addon.Type} for game {game} is not supported");
        }
    }
}
