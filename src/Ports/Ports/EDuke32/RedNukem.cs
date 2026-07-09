using System.Collections.Immutable;
using System.Text;
using Addons.Addons;
using Core.All.Enums;
using Core.All.Enums.Addons;
using Core.All.Enums.Versions;
using Games.Games;
using Microsoft.Extensions.Logging;

namespace Ports.Ports.EDuke32;

/// <summary>
///     RedNukem port.
/// </summary>
public sealed class RedNukem : EDuke32
{
    private readonly ILogger<RedNukem> _logger = null!;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RedNukem" /> class.
    /// </summary>
    public RedNukem() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RedNukem" /> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public RedNukem(ILogger<RedNukem> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public override PortEnum PortEnum => PortEnum.RedNukem;

    /// <inheritdoc />
    protected override string WinExe => "rednukem.exe";

    /// <inheritdoc />
    protected override string LinExe => throw new NotSupportedException();

    /// <inheritdoc />
    public override string Name => "RedNukem";

    /// <inheritdoc />
    protected override string AddGrpParam => "-g ";

    /// <inheritdoc />
    public override List<GameEnum> SupportedGames =>
    [
        GameEnum.Duke3D,
        GameEnum.Redneck,
        GameEnum.RidesAgain,
        GameEnum.NAM,
        GameEnum.WW2GI,
        GameEnum.Duke64
    ];

    /// <inheritdoc />
    public override List<string> SupportedGamesVersions => [nameof(DukeVersionEnum.Duke3D_Atomic)];

    /// <inheritdoc />
    public override List<FeatureEnum> SupportedFeatures =>
    [
        FeatureEnum.Hightile,
        FeatureEnum.Models,
        FeatureEnum.TileFromTexture
    ];

    /// <inheritdoc />
    protected override string ConfigFile => "rednukem.cfg";


    /// <inheritdoc />
    public override void BeforeStart(BaseGame game, BaseAddon campaign)
    {
        CreateBlankDemo();
        CreateOrDeleteBlankAnm(true);
        MoveSaveFilesFromStorage(game, campaign);
        FixConfig();
        FixRoute66Files(game, campaign);
        FixWtFiles(game, campaign);
    }


    /// <inheritdoc />
    protected override void GetStartCampaignArgs(StringBuilder sb, BaseGame game, BaseAddon addon)
    {
        //don't search for steam/gog installs
        _ = sb.Append(" -usecwd");
        _ = sb.Append(" -d blank.edm");

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
            throw new NotSupportedException($"Mod type {addon.Type} for game {game} is not supported");
        }
    }

    /// <inheritdoc />
    protected override void GetSkipIntroParameter(StringBuilder sb)
    {
        _ = sb.Append(" -quick");
        CreateOrDeleteBlankAnm(false);
    }

    /// <summary>
    ///     Creates or deletes blank animation files to skip or restore intros.
    /// </summary>
    /// <param name="isDelete"><see langword="true" /> to delete the files; <see langword="false" /> to create them.</param>
    private void CreateOrDeleteBlankAnm(bool isDelete)
    {
        ImmutableArray<string> files =
        [
            Path.Combine(InstallFolderPath, "LOGO.ANM"),
            Path.Combine(InstallFolderPath, "XATLOGO.ANM"),
            Path.Combine(InstallFolderPath, "REDNECK.ANM"),
            Path.Combine(InstallFolderPath, "RR_INTRO.ANM"),
            Path.Combine(InstallFolderPath, "REDINT.MVE")
        ];

        foreach (var file in files)
        {
            if (isDelete)
            {
                if (File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        if (_logger is not null)
                        {
                            _logger.LogWarning(ex, "Failed to delete {File}", file);
                        }
                    }
                }
            }
            else
            {
                if (!File.Exists(file))
                {
                    try
                    {
                        using var _ = File.CreateText(file);
                    }
                    catch (Exception ex)
                    {
                        if (_logger is not null)
                        {
                            _logger.LogWarning(ex, "Failed to create blank file {File}", file);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Appends command-line arguments for Redneck Rampage games in RedNukem.
    /// </summary>
    /// <param name="sb">String builder for parameters.</param>
    /// <param name="game">Redneck game instance.</param>
    /// <param name="addon">Campaign or addon.</param>
    private void GetRedneckArgs(StringBuilder sb, RedneckGame game, BaseAddon addon)
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

        if (addon.FileInfo is null)
        {
            return;
        }

        if (addon is LooseMap)
        {
            GetLooseMapArgs(sb, game, addon);

            return;
        }

        if (addon is not DukeCampaign rCamp)
        {
            throw new InvalidCastException();
        }

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
                _ = sb.Append($@" {AddFileParam}""{rCamp.FileInfo.PathToFile}""");
            }
        }
        else if (rCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, rCamp);
        }
        else
        {
            throw new NotSupportedException($"Mod type {rCamp.Type} is not supported");
        }
    }


    /// <summary>
    ///     Copies or restores Route 66 art and video files for RedNukem.
    /// </summary>
    /// <param name="game">Game instance.</param>
    /// <param name="campaign">Campaign or addon.</param>
    [Obsolete("Remove if RedNukem can ever properly launch R66")]
    private void FixRoute66Files(BaseGame game, BaseAddon campaign)
    {
        if (game is not RedneckGame)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(game.GameInstallFolder);

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


        if (campaign.AddonId.Id.Equals(nameof(RedneckAddonEnum.Route66), StringComparison.OrdinalIgnoreCase))
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

    /// <summary>
    ///     Creates a blank demo file to prevent demo playback on startup.
    /// </summary>
    private void CreateBlankDemo()
    {
        var blankDemo = Path.Combine(InstallFolderPath, "blank.edm");

        if (!File.Exists(blankDemo))
        {
            try
            {
                using var _ = File.CreateText(blankDemo);
            }
            catch (Exception ex)
            {
                if (_logger is not null)
                {
                    _logger.LogWarning(ex, "Failed to create blank demo file {File}", blankDemo);
                }
            }
        }
    }
}
