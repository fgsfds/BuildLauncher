using Common.Enums;
using Common.Enums.Addons;
using Common.Enums.Versions;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
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
    public override string Exe => "rednukem.exe";

    /// <inheritdoc/>
    public override string Name => "RedNukem";

    /// <inheritdoc/>
    public override List<GameEnum> SupportedGames =>
        [
        GameEnum.Duke3D,
        GameEnum.Redneck,
        GameEnum.RidesAgain,
        GameEnum.NAM,
        GameEnum.WWIIGI,
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
    protected override void BeforeStart(IGame game, IAddon campaign)
    {
        FixGrpInConfig();

        FixRoute66Files(game, campaign);
    }


    /// <inheritdoc/>
    protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon)
    {
        //don't search for steam/gog installs
        sb.Append(" -usecwd");

        if (addon.MainDef is not null)
        {
            sb.Append($@" {MainDefParam}""{addon.MainDef}""");
        }
        else
        {
            //overriding default def so gamename.def files are ignored
            sb.Append($@" {MainDefParam}""a""");
        }

        if (addon.AdditionalDefs is not null)
        {
            foreach (var def in addon.AdditionalDefs)
            {
                sb.Append($@" {AddDefParam}""{def}""");
            }
        }


        if (game is DukeGame dGame )
        {
            GetDukeArgs(sb, dGame, addon);
        }
        else if (game is RedneckGame rGame)
        {
            GetRedneckArgs(sb, rGame, addon);
        }
        else
        {
            ThrowHelper.NotImplementedException($"Mod type {addon.Type} for game {game} is not supported");
        }
    }


    /// <summary>
    /// Get startup agrs for Redneck Rampage
    /// </summary>
    /// <param name="sb">StringBuilder</param>
    /// <param name="game">RedneckGame</param>
    /// <param name="camp">RedneckCampaign</param>
    private void GetRedneckArgs(StringBuilder sb, RedneckGame game, IAddon addon)
    {
        if (addon.SupportedGame.GameEnum is GameEnum.RidesAgain)
        {
            sb.Append($@" {AddDirectoryParam}""{game.AgainInstallPath}""");
        }
        else if (addon.DependentAddons is not null &&
                 addon.DependentAddons.ContainsKey(nameof(RedneckAddonEnum.Route66)))
        {
            sb.Append($@" {AddDirectoryParam}""{game.GameInstallFolder}"" -x GAME66.CON");
        }
        else
        {
            sb.Append($@" {AddDirectoryParam}""{game.GameInstallFolder}""");
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

        if (addon is not RedneckCampaign rCamp)
        {
            ThrowHelper.ArgumentException(nameof(addon));
            return;
        }


        if (rCamp.MainCon is not null)
        {
            sb.Append($@" {MainConParam}""{rCamp.MainCon}""");
        }

        if (rCamp.AdditionalCons?.Count > 0)
        {
            foreach (var con in rCamp.AdditionalCons)
            {
                sb.Append($@" {AddConParam}""{con}""");
            }
        }


        if (rCamp.Type is AddonTypeEnum.TC)
        {
            sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, rCamp.FileName)}""");
        }
        else if (rCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, game, rCamp);
        }
        else
        {
            ThrowHelper.NotImplementedException($"Mod type {rCamp.Type} is not supported");
            return;
        }
    }


    /// <summary>
    /// Override original art files with route 66's ones or remove overrides
    /// </summary>
    [Obsolete("Remove if RedNukem can ever properly launch R66")]
    private static void FixRoute66Files(IGame game, IAddon campaign)
    {
        if (game is not RedneckGame rGame || !rGame.IsRoute66Installed)
        {
            return;
        }

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
            if (File.Exists(tilesA2))
            {
                File.Delete(tilesA2);
            }

            if (File.Exists(tilesB2))
            {
                File.Delete(tilesB2);
            }

            if (File.Exists(turdMovAnm2))
            {
                File.Delete(turdMovAnm2);
            }

            if (File.Exists(turdMovVoc2))
            {
                File.Delete(turdMovVoc2);
            }

            if (File.Exists(endMovAnm2))
            {
                File.Delete(endMovAnm2);
            }

            if (File.Exists(endMovVoc2))
            {
                File.Delete(endMovVoc2);
            }
        }
    }
}
