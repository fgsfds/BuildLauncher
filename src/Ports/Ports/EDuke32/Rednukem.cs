using System.Collections.Immutable;
using Addons.Addons;
using Core.All.Enums;
using Core.All.Enums.Addons;
using Core.All.Enums.Versions;
using Games.Games;
using Microsoft.Extensions.Logging;
using Ports.Builders;
using Ports.Helpers;

namespace Ports.Ports.EDuke32;

/// <summary>
///     Rednukem port.
/// </summary>
public sealed class Rednukem : EDuke32
{
    private readonly ILogger<Rednukem> _logger = null!;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Rednukem" /> class.
    /// </summary>
    public Rednukem() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Rednukem" /> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public Rednukem(ILogger<Rednukem> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public override PortEnum PortEnum => PortEnum.Rednukem;

    /// <inheritdoc />
    protected override string WinExe => "rednukem.exe";

    /// <inheritdoc />
    protected override string LinExe => throw new NotSupportedException();

    /// <inheritdoc />
    public override string Name => "Rednukem";

    /// <inheritdoc />
    public override PortCmdArguments CmdArguments => base.CmdArguments with
    {
        AddGrp = "-g "
    };

    /// <inheritdoc />
    public override ImmutableHashSet<GameEnum> SupportedGames { get; } =
    [
        GameEnum.Duke3D,
        GameEnum.Redneck,
        GameEnum.RidesAgain,
        GameEnum.NAM,
        GameEnum.WW2GI,
        GameEnum.Duke64
    ];

    /// <inheritdoc />
    public override ImmutableHashSet<string> SupportedGamesVersions { get; } = [nameof(DukeVersionEnum.Duke3D_Atomic)];

    /// <inheritdoc />
    public override ImmutableHashSet<FeatureEnum> SupportedFeatures { get; } =
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
        SaveFilesHelper.MoveSaveFilesFromStorage(
            GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id),
            GetGameSaveFilesFolder(game, campaign));
        FixConfig();
        FixRoute66Files(game, campaign);
        FixWtFiles(game, campaign);
    }

    /// <inheritdoc />
    protected override void CustomModifyArgs(CmdParametersBuilder sb, BaseGame game, BaseAddon addon)
    {
        _ = sb.AppendSkipSteam();
    }

    /// <summary>
    ///     Creates or deletes blank animation files to skip or restore intros.
    /// </summary>
    /// <param name="isDelete"><see langword="true" /> to delete the files; <see langword="false" /> to create them.</param>
    internal void CreateOrDeleteBlankAnm(bool isDelete)
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
    ///     Copies or restores Route 66 art and video files for Rednukem.
    /// </summary>
    /// <param name="game">Game instance.</param>
    /// <param name="campaign">Campaign or addon.</param>
    [Obsolete("Remove if Rednukem can ever properly launch R66")]
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
            FilesHelpers.RestoreRoute66Files(game);
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
