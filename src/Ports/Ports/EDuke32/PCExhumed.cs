﻿using Common.Enums;
using Common.Interfaces;
using CommunityToolkit.Diagnostics;
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
    protected override string WinExe => "pcexhumed.exe";

    /// <inheritdoc/>
    protected override string LinExe => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    public override string Name => "PCExhumed";

    /// <inheritdoc/>
    public override List<GameEnum> SupportedGames => [GameEnum.Exhumed];

    /// <inheritdoc/>
    public override List<FeatureEnum> SupportedFeatures => [];

    /// <inheritdoc/>
    protected override string ConfigFile => "pcexhumed.cfg";


    /// <inheritdoc/>
    public override void BeforeStart(IGame game, IAddon campaign)
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


        if (game is SlaveGame sGame)
        {
            GetSlaveArgs(sb, sGame, addon);
        }
        else
        {
            ThrowHelper.ThrowNotSupportedException($"Mod type {addon.Type} for game {game} is not supported");
        }
    }
}
