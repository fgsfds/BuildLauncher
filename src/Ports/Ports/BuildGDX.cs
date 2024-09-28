using Common;
using Common.Enums;
using Common.Enums.Versions;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using System.Text;

namespace Ports.Ports;

/// <summary>
/// BuildGDX port
/// </summary>
public sealed class BuildGDX : BasePort
{
    /// <inheritdoc/>
    public override PortEnum PortEnum => PortEnum.BuildGDX;

    /// <inheritdoc/>
    public override string Exe => Path.Combine("jre", "bin", "javaw.exe");

    /// <inheritdoc/>
    public override string Name => "BuildGDX";

    /// <inheritdoc/>
    public override List<GameEnum> SupportedGames =>
        [
        GameEnum.Blood,
        GameEnum.Duke3D,
        GameEnum.ShadowWarrior,
        GameEnum.Exhumed,
        GameEnum.Redneck,
        GameEnum.RidesAgain,
        GameEnum.NAM,
        GameEnum.WWIIGI,
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
            var versionFile = Path.Combine(PortExecutableFolderPath, "version");

            if (!File.Exists(versionFile))
            {
                return null;
            }

            return File.ReadAllText(versionFile);
        }
    }

    /// <inheritdoc/>
    public override bool IsInstalled => File.Exists(Path.Combine(PortExecutableFolderPath, "BuildGDX.jar"));

    /// <inheritdoc/>
    public override List<FeatureEnum> SupportedFeatures =>
            [
            FeatureEnum.TROR,
            FeatureEnum.Hightile,
            FeatureEnum.Models
            ];


    /// <inheritdoc/>
    protected override string ConfigFile => string.Empty;

    /// <inheritdoc/>
    protected override string AddDirectoryParam => ThrowHelper.NotImplementedException<string>();

    /// <inheritdoc/>
    protected override string AddFileParam => ThrowHelper.NotImplementedException<string>();

    /// <inheritdoc/>
    protected override string AddDefParam => ThrowHelper.NotImplementedException<string>();

    /// <inheritdoc/>
    protected override string AddConParam => ThrowHelper.NotImplementedException<string>();

    /// <inheritdoc/>
    protected override string MainDefParam => ThrowHelper.NotImplementedException<string>();

    /// <inheritdoc/>
    protected override string MainConParam => ThrowHelper.NotImplementedException<string>();

    /// <inheritdoc/>
    protected override string AddGrpParam => ThrowHelper.NotImplementedException<string>();

    /// <inheritdoc/>
    protected override string SkillParam => ThrowHelper.NotImplementedException<string>();

    /// <inheritdoc/>
    protected override string AddGameDirParam => ThrowHelper.NotImplementedException<string>();

    /// <inheritdoc/>
    protected override string AddRffParam => ThrowHelper.NotImplementedException<string>();

    /// <inheritdoc/>
    protected override string AddSndParam => ThrowHelper.NotImplementedException<string>();


    /// <inheritdoc/>
    protected override void BeforeStart(IGame game, IAddon campaign)
    {
        //nothing to do
    }

    /// <inheritdoc/>
    public override void AfterStart(IGame game, IAddon campaign)
    {
        //nothing to do
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
        else
        {
            ThrowHelper.NotImplementedException($"Mod type {addon} for game {game} is not supported");
        }
    }

    /// <inheritdoc/>
    protected override void GetAutoloadModsArgs(StringBuilder sb, IGame _, IAddon addon, Dictionary<AddonVersion, IAddon> mods) { }

    /// <inheritdoc/>
    protected override void GetSkipIntroParameter(StringBuilder sb) { }

    /// <inheritdoc/>
    protected override void GetSkipStartupParameter(StringBuilder sb) { }


    private void GetDukeArgs(StringBuilder sb, DukeGame game, IAddon camp)
    {
        if (camp.Id.Equals(nameof(DukeVersionEnum.Duke3D_WT), StringComparison.OrdinalIgnoreCase))
        {
            _ = sb.Append($@" -path ""{game.DukeWTInstallPath}""");
        }
        else
        {
            _ = sb.Append($@" -path ""{game.GameInstallFolder}""");
        }

        _ = sb.Append($@" -game DUKE_NUKEM_3D");
    }

    private new void GetBloodArgs(StringBuilder sb, BloodGame game, IAddon camp)
    {
        _ = sb.Append($@" -path ""{game.GameInstallFolder}""");

        _ = sb.Append($@" -game BLOOD");
    }

    private static void GetWangArgs(StringBuilder sb, WangGame game, IAddon camp)
    {
        _ = sb.Append($@" -path ""{game.GameInstallFolder}""");

        _ = sb.Append($@" -game SHADOW_WARRIOR");
    }

    private void GetRedneckArgs(StringBuilder sb, RedneckGame game, IAddon camp)
    {
        if (camp.Id.Equals(nameof(GameEnum.RidesAgain), StringComparison.OrdinalIgnoreCase))
        {
            _ = sb.Append($@" -path ""{game.AgainInstallPath}""");
            _ = sb.Append($@" -game RR_RIDES_AGAIN");
        }
        else
        {
            _ = sb.Append($@" -path ""{game.GameInstallFolder}""");
            _ = sb.Append($@" -game REDNECK_RAMPAGE");
        }
    }

    private new static void GetSlaveArgs(StringBuilder sb, SlaveGame game, IAddon camp)
    {
        _ = sb.Append($@" -path ""{game.GameInstallFolder}""");

        _ = sb.Append($@" -game POWERSLAVE");
    }
}
