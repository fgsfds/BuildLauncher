using System.Collections.Immutable;
using System.Text;
using Addons.Addons;
using Core.All.Enums;
using Games.Games;

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
    protected override PortCmdArguments CmdArguments => base.CmdArguments with
    {
        SkillLevel = "-s ",
        AddRff = "-rff ",
        AddSnd = "-snd "
    };


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


        if (game is BloodGame bGame)
        {
            GetBloodArgs(sb, bGame, addon);
        }
        else
        {
            throw new NotSupportedException($"Mod type {addon.Type} for game {game} is not supported");
        }
    }
}
