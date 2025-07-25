﻿using System.Text;
using Addons.Addons;
using Common.Client.Interfaces;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Diagnostics;
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
    protected override string LinExe => ThrowHelper.ThrowNotSupportedException<string>();

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
        FeatureEnum.Wall_Rotate_Cstat,
        FeatureEnum.TileFromTexture
        ];

    /// <inheritdoc/>
    protected override string ConfigFile => "fury.cfg";


    public Fury(IConfigProvider config)
    {
        _config = config;
    }


    /// <inheritdoc/>
    public override void BeforeStart(IGame game, IAddon campaign)
    {
        MoveSaveFilesFromGameFolder(game, campaign);

        FixGrpInConfig();
    }


    /// <inheritdoc/>
    protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon)
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
            ThrowHelper.ThrowNotSupportedException($"Mod type {addon.Type} for game {game} is not supported");
        }
    }

    private void GetFuryArgs(StringBuilder sb, FuryGame game, IAddon addon)
    {
        if (addon.FileName is null)
        {
            return;
        }

        if (addon is LooseMapEntity)
        {
            GetLooseMapArgs(sb, game, addon);
            return;
        }


        Guard2.ThrowIfNotType<DukeCampaignEntity>(addon, out var fCamp);

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
            ThrowHelper.ThrowNotSupportedException($"Mod type {fCamp.Type} is not supported");
            return;
        }
    }
}
