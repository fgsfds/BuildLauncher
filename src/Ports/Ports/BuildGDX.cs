﻿using System.Text;
using Common;
using Common.Enums;
using Common.Enums.Versions;
using Common.Interfaces;
using CommunityToolkit.Diagnostics;
using Games.Games;

namespace Ports.Ports;

/// <summary>
/// BuildGDX port
/// </summary>
public sealed class BuildGDX : BasePort
{
    /// <inheritdoc/>
    public override PortEnum PortEnum => PortEnum.BuildGDX;

    /// <inheritdoc/>
    protected override string WinExe => Path.Combine("jre", "bin", "javaw.exe");

    /// <inheritdoc/>
    protected override string LinExe => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    public override string Name => "BuildGDX";

    /// <inheritdoc/>
    public override List<GameEnum> SupportedGames =>
        [
        GameEnum.Blood,
        GameEnum.Duke3D,
        GameEnum.Wang,
        GameEnum.Slave,
        GameEnum.Redneck,
        GameEnum.RidesAgain,
        GameEnum.NAM,
        GameEnum.Witchaven,
        GameEnum.Witchaven2,
        GameEnum.TekWar
        ];

    /// <inheritdoc/>
    public override List<string> SupportedGamesVersions =>
        [
        nameof(DukeVersionEnum.Duke3D_13D),
        nameof(DukeVersionEnum.Duke3D_Atomic),
        nameof(DukeVersionEnum.Duke3D_WT)
        ];

    /// <inheritdoc/>
    public override string? InstalledVersion
    {
        get
        {
            var versionFile = Path.Combine(PortInstallFolderPath, "version");

            if (!File.Exists(versionFile))
            {
                return null;
            }

            return File.ReadAllText(versionFile);
        }
    }

    /// <inheritdoc/>
    public override List<FeatureEnum> SupportedFeatures =>
            [
            FeatureEnum.TROR,
            FeatureEnum.Hightile,
            FeatureEnum.Models,
            FeatureEnum.TileFromTexture
            ];


    /// <inheritdoc/>
    protected override string ConfigFile => string.Empty;

    /// <inheritdoc/>
    protected override string AddDirectoryParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string AddFileParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string AddDefParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string AddConParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string MainDefParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string MainConParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string MainGrpParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string AddGrpParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string SkillParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string AddGameDirParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string AddRffParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string AddSndParam => ThrowHelper.ThrowNotSupportedException<string>();


    /// <inheritdoc/>
    public override void BeforeStart(IGame game, IAddon campaign)
    {
        MoveSaveFilesToGameFolder(game, campaign);

        RestoreRoute66Files(game);

        RestoreWtFiles(game);
    }

    /// <inheritdoc/>
    public override void AfterEnd(IGame game, IAddon campaign)
    {
        MoveSaveFilesFromGameFolder(game, campaign);
    }

    /// <inheritdoc/>
    protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon)
    {
        _ = sb.Append(@" -jar ..\..\BuildGDX.jar");

        if (game is BloodGame bGame)
        {
            GetBloodArgs(sb, bGame, addon);
        }
        else if (game is DukeGame dGame)
        {
            GetDukeArgs(sb, dGame, addon);
        }
        else if (game is WangGame wGame)
        {
            GetWangArgs(sb, wGame, addon);
        }
        else if (game is SlaveGame sGame)
        {
            GetSlaveArgs(sb, sGame, addon);
        }
        else if (game is RedneckGame rGame)
        {
            GetRedneckArgs(sb, rGame, addon);
        }
        else if (game is NamGame nGame)
        {
            GetNamArgs(sb, nGame, addon);
        }
        else if (game is WitchavenGame whGame)
        {
            GetWitchavenArgs(sb, whGame, addon);
        }
        else if (game is TekWarGame tGame)
        {
            GetTekWarArgs(sb, tGame, addon);
        }
        else
        {
            ThrowHelper.ThrowNotSupportedException($"Mod type {addon} for game {game} is not supported");
        }
    }

    /// <inheritdoc/>
    protected override void GetAutoloadModsArgs(StringBuilder sb, IGame _, IAddon addon, IEnumerable<KeyValuePair<AddonId, IAddon>> mods) { }

    /// <inheritdoc/>
    protected override void GetSkipIntroParameter(StringBuilder sb) { }

    /// <inheritdoc/>
    protected override void GetSkipStartupParameter(StringBuilder sb) { }


    private void GetDukeArgs(StringBuilder sb, DukeGame game, IAddon camp)
    {
        if (camp.AddonId.Id.Equals(nameof(DukeVersionEnum.Duke3D_WT), StringComparison.OrdinalIgnoreCase))
        {
            _ = sb.Append($@" -path ""{game.DukeWTInstallPath}""");
        }
        else
        {
            _ = sb.Append($@" -path ""{game.GameInstallFolder}""");
        }

        _ = sb.Append(" -game DUKE_NUKEM_3D");
    }

    private new void GetBloodArgs(StringBuilder sb, BloodGame game, IAddon camp)
    {
        _ = sb.Append($@" -path ""{game.GameInstallFolder}""");

        _ = sb.Append(" -game BLOOD");
    }

    private static void GetWangArgs(StringBuilder sb, WangGame game, IAddon camp)
    {
        _ = sb.Append($@" -path ""{game.GameInstallFolder}""");

        _ = sb.Append(" -game SHADOW_WARRIOR");
    }

    private void GetRedneckArgs(StringBuilder sb, RedneckGame game, IAddon camp)
    {
        if (camp.AddonId.Id.Equals(nameof(GameEnum.RidesAgain), StringComparison.OrdinalIgnoreCase))
        {
            _ = sb.Append($@" -path ""{game.AgainInstallPath}""");
            _ = sb.Append(" -game RR_RIDES_AGAIN");
        }
        else
        {
            _ = sb.Append($@" -path ""{game.GameInstallFolder}""");
            _ = sb.Append(" -game REDNECK_RAMPAGE");
        }
    }

    private new static void GetSlaveArgs(StringBuilder sb, SlaveGame game, IAddon camp)
    {
        _ = sb.Append($@" -path ""{game.GameInstallFolder}""");

        _ = sb.Append(" -game POWERSLAVE");
    }

    private static void GetNamArgs(StringBuilder sb, NamGame game, IAddon camp)
    {
        _ = sb.Append($@" -path ""{game.GameInstallFolder}""");

        _ = sb.Append(" -game NAM");
    }

    private static void GetWitchavenArgs(StringBuilder sb, WitchavenGame game, IAddon camp)
    {
        if (camp.AddonId.Id.Equals(nameof(GameEnum.Witchaven2), StringComparison.OrdinalIgnoreCase))
        {
            _ = sb.Append($@" -path ""{game.Witchaven2InstallPath}""");
            _ = sb.Append(" -game WITCHAVEN_2");
        }
        else
        {
            _ = sb.Append($@" -path ""{game.GameInstallFolder}""");
            _ = sb.Append(" -game WITCHAVEN");
        }
    }

    private static void GetTekWarArgs(StringBuilder sb, TekWarGame game, IAddon camp)
    {
        _ = sb.Append($@" -path ""{game.GameInstallFolder}""");

        _ = sb.Append(" -game TEKWAR");
    }
}
