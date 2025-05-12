using Addons.Addons;
using Common.Enums;
using Common.Enums.Addons;
using Common.Enums.Versions;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Diagnostics;
using Games.Games;
using System.Text;

namespace Ports.Ports.EDuke32;

/// <summary>
/// RedNukem port
/// </summary>
public sealed class RedNukem : EDuke32
{
    /// <inheritdoc/>
    public override PortEnum PortEnum => PortEnum.RedNukem;

    /// <inheritdoc/>
    protected override string WinExe => "rednukem.exe";

    /// <inheritdoc/>
    protected override string LinExe => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    public override string Name => "RedNukem";

    /// <inheritdoc/>
    protected override string AddGrpParam => "-g ";

    /// <inheritdoc/>
    public override List<GameEnum> SupportedGames =>
        [
        GameEnum.Duke3D,
        GameEnum.Redneck,
        GameEnum.RidesAgain,
        GameEnum.NAM,
        GameEnum.WW2GI,
        GameEnum.Duke64
        ];

    /// <inheritdoc/>
    public override List<string> SupportedGamesVersions =>
        [
        nameof(DukeVersionEnum.Duke3D_Atomic)
        ];

    /// <inheritdoc/>
    public override List<FeatureEnum> SupportedFeatures =>
        [
        FeatureEnum.Hightile,
        FeatureEnum.Models
        ];

    /// <inheritdoc/>
    protected override string ConfigFile => "rednukem.cfg";


    /// <inheritdoc/>
    public override void BeforeStart(IGame game, IAddon campaign)
    {
        MoveSaveFiles(game, campaign);

        FixGrpInConfig();

        FixRoute66Files(game, campaign);

        FixWtFiles(game, campaign);
    }


    /// <inheritdoc/>
    protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon)
    {
        //don't search for steam/gog installs
        _ = sb.Append(" -usecwd");

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


        if (game is DukeGame dGame)
        {
            GetDukeArgs(sb, dGame, addon);
        }
        else if (game is RedneckGame rGame)
        {
            GetRedneckArgs(sb, rGame, addon);
        }
        else if (game is NamGame nGame)
        {
            _ = sb.Append($@" {AddDirectoryParam}""{game.GameInstallFolder}""");

            GetNamWW2GIArgs(sb, nGame, addon);
        }
        else if (game is WW2GIGame giGame)
        {
            _ = sb.Append($@" {AddDirectoryParam}""{game.GameInstallFolder}""");

            GetNamWW2GIArgs(sb, giGame, addon);
        }
        else
        {
            ThrowHelper.ThrowNotSupportedException($"Mod type {addon.Type} for game {game} is not supported");
        }
    }


    /// <summary>
    /// Get startup agrs for Redneck Rampage
    /// </summary>
    /// <param name="sb">StringBuilder</param>
    /// <param name="game">RedneckGame</param>
    /// <param name="addon">DukeCampaign</param>
    private void GetRedneckArgs(StringBuilder sb, RedneckGame game, IAddon addon)
    {
        if (addon.SupportedGame.GameEnum is GameEnum.RidesAgain)
        {
            _ = sb.Append($@" {AddDirectoryParam}""{game.AgainInstallPath}""");
        }
        else if (addon.DependentAddons?.ContainsKey(nameof(RedneckAddonEnum.Route66)) is true)
        {
            _ = sb.Append($@" {AddDirectoryParam}""{game.GameInstallFolder}"" -x GAME66.CON");
        }
        else
        {
            _ = sb.Append($@" {AddDirectoryParam}""{game.GameInstallFolder}""");
        }


        if (addon.FileName is null)
        {
            return;
        }

        if (addon is LooseMap)
        {
            GetLooseMapArgs(sb, game, addon);
            return;
        }


        addon.ThrowIfNotType<DukeCampaign>(out var rCamp);

        if (rCamp.MainCon is not null)
        {
            _ = sb.Append($@" {MainConParam}""{rCamp.MainCon}""");
        }

        if (rCamp.AdditionalCons?.Any() is true)
        {
            foreach (var con in rCamp.AdditionalCons)
            {
                _ = sb.Append($@" {AddConParam}""{con}""");
            }
        }


        if (rCamp.Type is AddonTypeEnum.TC)
        {
            if (rCamp.Executables is not null)
            {
                //don't add addon dir if the port is overridden
            }
            else
            {
                _ = sb.Append($@" {AddFileParam}""{rCamp.PathToFile}""");
            }
        }
        else if (rCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, rCamp);
        }
        else
        {
            ThrowHelper.ThrowNotSupportedException($"Mod type {rCamp.Type} is not supported");
            return;
        }
    }


    /// <summary>
    /// Override original art files with route 66's ones or remove overrides
    /// </summary>
    [Obsolete("Remove if RedNukem can ever properly launch R66")]
    private void FixRoute66Files(IGame game, IAddon campaign)
    {
        if (game is not RedneckGame)
        {
            return;
        }

        Guard.IsNotNull(game.GameInstallFolder);

        var tilesA1 = Path.Combine(game.GameInstallFolder, "TILESA66.ART");
        var tilesA2 = Path.Combine(game.GameInstallFolder, "TILES024.ART");

        var tilesB1 = Path.Combine(game.GameInstallFolder, "TILESB66.ART");
        var tilesB2 = Path.Combine(game.GameInstallFolder, "TILES025.ART");

        var turdMovAnm1 = Path.Combine(game.GameInstallFolder, "TURD66.ANM");
        var turdMovAnm2 = Path.Combine(game.GameInstallFolder, "TURDMOV.ANM");

        var turdMovVoc1 = Path.Combine(game.GameInstallFolder, "TURD66.VOC");
        var turdMovVoc2 = Path.Combine(game.GameInstallFolder, "TURDMOV.VOC");

        var endMovAnm1 = Path.Combine(game.GameInstallFolder, "END66.ANM");
        var endMovAnm2 = Path.Combine(game.GameInstallFolder, "RR_OUTRO.ANM");

        var endMovVoc1 = Path.Combine(game.GameInstallFolder, "END66.VOC");
        var endMovVoc2 = Path.Combine(game.GameInstallFolder, "LN_FINAL.VOC");


        if (campaign.Id.Equals(nameof(RedneckAddonEnum.Route66), StringComparison.OrdinalIgnoreCase))
        {
            File.Copy(tilesA1, tilesA2, true);
            File.Copy(tilesB1, tilesB2, true);
            File.Copy(turdMovAnm1, turdMovAnm2, true);
            File.Copy(turdMovVoc1, turdMovVoc2, true);
            File.Copy(endMovAnm1, endMovAnm2, true);
            File.Copy(endMovVoc1, endMovVoc2, true);
        }
        else
        {
            RestoreRoute66Files(game);
        }
    }
}
