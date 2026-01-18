using System.Text;
using Addons.Addons;
using Common.All.Enums;
using Common.Client.Interfaces;
using Games.Games;

namespace Ports.Ports.EDuke32;

/// <summary>
/// RedNukem port
/// </summary>
public sealed class Fury : EDuke32
{
    private readonly IConfigProvider _config;

    /// <inheritdoc/>
    public override PortEnum PortEnum => PortEnum.Fury;

    /// <inheritdoc/>
    protected override string WinExe => "fury.exe";

    /// <inheritdoc/>
    protected override string LinExe => throw new NotSupportedException();

    /// <inheritdoc/>
    public override string Name => "Fury";

    /// <inheritdoc/>
    public override List<GameEnum> SupportedGames => [GameEnum.Fury];

    /// <inheritdoc/>
    public override string InstallFolderPath => _config.PathFury ?? string.Empty;

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
        FeatureEnum.Wall_Rotate_Cstat,
        FeatureEnum.TileFromTexture
        ];

    /// <inheritdoc/>
    protected override string ConfigFile => "fury.cfg";

    /// <inheritdoc/>
    public override bool IsDownloadable => false;


    public Fury(IConfigProvider config)
    {
        _config = config;
    }


    /// <inheritdoc/>
    public override void BeforeStart(BaseGame game, BaseAddon campaign)
    {
        MoveSaveFilesFromGameFolder(game, campaign);

        FixConfig();
    }


    /// <inheritdoc/>
    protected override void GetStartCampaignArgs(StringBuilder sb, BaseGame game, BaseAddon addon)
    {
        if (addon.MainDef is not null)
        {
            _ = sb.Append($@" {MainDefParam}""{addon.MainDef}""");
        }
        //no need to override main def

        if (addon.AdditionalDefs is not null)
        {
            foreach (var def in addon.AdditionalDefs)
            {
                _ = sb.Append($@" {AddDefParam}""{def}""");
            }
        }


        if (game is FuryGame fGame)
        {
            GetFuryArgs(sb, fGame, addon);
        }
        else
        {
            throw new NotSupportedException($"Mod type {addon.Type} for game {game} is not supported");
        }
    }

    private void GetFuryArgs(StringBuilder sb, FuryGame game, BaseAddon addon)
    {
        if (addon.FileName is null)
        {
            return;
        }

        if (addon is LooseMap)
        {
            GetLooseMapArgs(sb, game, addon);
            return;
        }

        if (addon is not DukeCampaign fCamp)
        {
            throw new InvalidCastException();
        }

        if (fCamp.MainCon is not null)
        {
            _ = sb.Append($@" {MainConParam}""{fCamp.MainCon}""");
        }

        if (fCamp.AdditionalCons?.Any() is true)
        {
            foreach (var con in fCamp.AdditionalCons)
            {
                _ = sb.Append($@" {AddConParam}""{con}""");
            }
        }


        if (fCamp.Type is AddonTypeEnum.TC)
        {
            _ = sb.Append($@" {AddFileParam}""{fCamp.PathToFile}""");
        }
        else if (fCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, fCamp);
        }
        else
        {
            throw new NotSupportedException($"Mod type {fCamp.Type} is not supported");
            return;
        }
    }
}
