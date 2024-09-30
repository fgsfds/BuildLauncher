using Common.Client.Config;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
using System.Text;

namespace Ports.Ports.EDuke32;

/// <summary>
/// RedNukem port
/// </summary>
public sealed class Fury(IConfigProvider config) : EDuke32
{
    private readonly IConfigProvider _config = config;

    /// <inheritdoc/>
    public override PortEnum PortEnum => PortEnum.Fury;

    /// <inheritdoc/>
    public override string Exe => "fury.exe";

    /// <inheritdoc/>
    public override string Name => "Fury";

    /// <inheritdoc/>
    public override List<GameEnum> SupportedGames => [GameEnum.Fury];

    /// <inheritdoc/>
    public override string PortInstallFolderPath => _config.PathFury ?? string.Empty;

    /// <inheritdoc/>
    public override bool IsInstalled => File.Exists(PortExeFilePath);

    /// <inheritdoc/>
    public override List<FeatureEnum> SupportedFeatures =>
        [
        FeatureEnum.EDuke32_CON,
        FeatureEnum.Dynamic_Lighting,
        FeatureEnum.Hightile,
        FeatureEnum.Models,
        FeatureEnum.Sloped_Sprites,
        FeatureEnum.TROR,
        FeatureEnum.Wall_Rotate_Cstat
        ];

    /// <inheritdoc/>
    protected override string ConfigFile => "fury.cfg";


    /// <inheritdoc/>
    protected override void BeforeStart(IGame game, IAddon campaign)
    {
        MoveSaveFiles(game, campaign);

        FixGrpInConfig();
    }


    /// <inheritdoc/>
    protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon)
    {
        if (addon.MainDef is not null)
        {
            sb.Append($@" {MainDefParam}""{addon.MainDef}""");
        }
        //no need to override main def

        if (addon.AdditionalDefs is not null)
        {
            foreach (var def in addon.AdditionalDefs)
            {
                sb.Append($@" {AddDefParam}""{def}""");
            }
        }


        if (game is FuryGame fGame)
        {
            GetFuryArgs(sb, fGame, addon);
        }
        else
        {
            ThrowHelper.NotImplementedException($"Mod type {addon.Type} for game {game} is not supported");
        }
    }

    private void GetFuryArgs(StringBuilder sb, FuryGame game, IAddon addon)
    {
        if (addon is LooseMap)
        {
            GetLooseMapArgs(sb, game, addon);
            return;
        }

        if (addon is not FuryCampaign fCamp)
        {
            ThrowHelper.ArgumentException(nameof(addon));
            return;
        }

        if (fCamp.FileName is null)
        {
            return;
        }


        if (fCamp.MainCon is not null)
        {
            sb.Append($@" {MainConParam}""{fCamp.MainCon}""");
        }

        if (fCamp.AdditionalCons?.Count > 0)
        {
            foreach (var con in fCamp.AdditionalCons)
            {
                sb.Append($@" {AddConParam}""{con}""");
            }
        }


        if (fCamp.Type is AddonTypeEnum.TC)
        {
            sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, fCamp.FileName)}""");
        }
        else if (fCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, game, fCamp);
        }
        else
        {
            ThrowHelper.NotImplementedException($"Mod type {fCamp.Type} is not supported");
            return;
        }
    }
}
