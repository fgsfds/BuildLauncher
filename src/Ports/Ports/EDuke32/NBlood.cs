using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
using System.Text;

namespace Ports.Ports.EDuke32;

/// <summary>
/// NBlood port
/// </summary>
public class NBlood : EDuke32
{
    /// <inheritdoc/>
    public override PortEnum PortEnum => PortEnum.NBlood;

    /// <inheritdoc/>
    public override string Exe => "nblood.exe";

    /// <inheritdoc/>
    public override string Name => "NBlood";

    /// <inheritdoc/>
    public override List<GameEnum> SupportedGames => [GameEnum.Blood];

    /// <inheritdoc/>
    public override List<FeatureEnum> SupportedFeatures =>
        [
        FeatureEnum.Modern_Types,
        FeatureEnum.Hightile,
        FeatureEnum.Models
        ];

    /// <inheritdoc/>
    protected override string ConfigFile => "nblood.cfg";

    /// <inheritdoc/>
    protected override string SkillParam => "-s ";

    /// <inheritdoc/>
    protected override string AddRffParam => "-rff ";

    /// <inheritdoc/>
    protected override string AddSndParam => "-snd ";


    /// <inheritdoc/>
    protected override void BeforeStart(IGame game, IAddon campaign)
    {
        MoveSaveFiles(game, campaign);
    }


    /// <inheritdoc/>
    protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon)
    {
        //don't search for steam/gog installs
        _ = sb.Append(" -usecwd");

        _ = sb.Append(@$" {AddDirectoryParam}""{game.GameInstallFolder}""");

        if (addon.MainDef is not null)
        {
            _ = sb.Append($@" {MainDefParam}""{addon.MainDef}""");
        }
        else
        {
            //overriding default def so gamename.def files are ignored
            _ = sb.Append($@" {MainDefParam}""a""");
        }

        if (addon.AdditionalDefs is not null)
        {
            foreach (var def in addon.AdditionalDefs)
            {
                _ = sb.Append($@" {AddDefParam}""{def}""");
            }
        }


        if (game is BloodGame bGame)
        {
            GetBloodArgs(sb, bGame, addon);
        }
        else
        {
            ThrowHelper.NotImplementedException($"Mod type {addon.Type} for game {game} is not supported");
        }
    }
}
