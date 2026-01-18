using System.Text;
using Addons.Addons;
using Common.All;
using Common.All.Enums;
using Common.All.Enums.Versions;
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
    protected override string LinExe => throw new NotSupportedException();

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
            var versionFile = Path.Combine(InstallFolderPath, "version");

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
    public override bool IsSkillSelectionAvailable => false;


    /// <inheritdoc/>
    protected override string ConfigFile => string.Empty;

    /// <inheritdoc/>
    protected override string AddDirectoryParam => throw new NotSupportedException();

    /// <inheritdoc/>
    protected override string AddFileParam => throw new NotSupportedException();

    /// <inheritdoc/>
    protected override string AddDefParam => throw new NotSupportedException();

    /// <inheritdoc/>
    protected override string AddConParam => throw new NotSupportedException();

    /// <inheritdoc/>
    protected override string MainDefParam => throw new NotSupportedException();

    /// <inheritdoc/>
    protected override string MainConParam => throw new NotSupportedException();

    /// <inheritdoc/>
    protected override string MainGrpParam => throw new NotSupportedException();

    /// <inheritdoc/>
    protected override string AddGrpParam => throw new NotSupportedException();

    /// <inheritdoc/>
    protected override string SkillParam => throw new NotSupportedException();

    /// <inheritdoc/>
    protected override string AddGameDirParam => throw new NotSupportedException();

    /// <inheritdoc/>
    protected override string AddRffParam => throw new NotSupportedException();

    /// <inheritdoc/>
    protected override string AddSndParam => throw new NotSupportedException();


    /// <inheritdoc/>
    public override void BeforeStart(BaseGame game, BaseAddon campaign)
    {
        MoveSaveFilesToGameFolder(game, campaign);

        RestoreRoute66Files(game);

        RestoreWtFiles(game);
    }

    /// <inheritdoc/>
    public override void AfterEnd(BaseGame game, BaseAddon campaign)
    {
        MoveSaveFilesFromGameFolder(game, campaign);
    }

    /// <inheritdoc/>
    protected override void GetStartCampaignArgs(StringBuilder sb, BaseGame game, BaseAddon addon)
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
            throw new NotSupportedException($"Mod type {addon} for game {game} is not supported");
        }
    }

    /// <inheritdoc/>
    protected override void GetAutoloadModsArgs(StringBuilder sb, BaseGame _, BaseAddon addon, IReadOnlyDictionary<AddonId, BaseAddon> mods) { }

    /// <inheritdoc/>
    protected override void GetSkipIntroParameter(StringBuilder sb) { }

    /// <inheritdoc/>
    protected override void GetSkipStartupParameter(StringBuilder sb) { }


    private void GetDukeArgs(StringBuilder sb, DukeGame game, BaseAddon camp)
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

    private new void GetBloodArgs(StringBuilder sb, BloodGame game, BaseAddon camp)
    {
        _ = sb.Append($@" -path ""{game.GameInstallFolder}""");

        _ = sb.Append(" -game BLOOD");
    }

    private static void GetWangArgs(StringBuilder sb, WangGame game, BaseAddon camp)
    {
        _ = sb.Append($@" -path ""{game.GameInstallFolder}""");

        _ = sb.Append(" -game SHADOW_WARRIOR");
    }

    private void GetRedneckArgs(StringBuilder sb, RedneckGame game, BaseAddon camp)
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

    private new static void GetSlaveArgs(StringBuilder sb, SlaveGame game, BaseAddon camp)
    {
        _ = sb.Append($@" -path ""{game.GameInstallFolder}""");

        _ = sb.Append(" -game POWERSLAVE");
    }

    private static void GetNamArgs(StringBuilder sb, NamGame game, BaseAddon camp)
    {
        _ = sb.Append($@" -path ""{game.GameInstallFolder}""");

        _ = sb.Append(" -game NAM");
    }

    private static void GetWitchavenArgs(StringBuilder sb, WitchavenGame game, BaseAddon camp)
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

    private static void GetTekWarArgs(StringBuilder sb, TekWarGame game, BaseAddon camp)
    {
        _ = sb.Append($@" -path ""{game.GameInstallFolder}""");

        _ = sb.Append(" -game TEKWAR");
    }
}
