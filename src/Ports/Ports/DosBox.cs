using System.Collections.Immutable;
using Addons.Addons;
using Core.All.Enums;
using Core.All.Enums.Versions;
using Games.Games;
using Microsoft.Extensions.Logging;
using Ports.Builders;
using Ports.Helpers;

namespace Ports.Ports;

/// <summary>
///     DosBox Staging port.
/// </summary>
public sealed class DosBox : BasePort
{
    private readonly ILogger<DosBox> _logger = null!;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DosBox" /> class.
    /// </summary>
    public DosBox() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DosBox" /> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public DosBox(ILogger<DosBox> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public override PortEnum PortEnum => PortEnum.DosBox;

    /// <inheritdoc />
    protected override string WinExe => "dosbox.exe";

    /// <inheritdoc />
    protected override string LinExe => throw new NotSupportedException();

    /// <inheritdoc />
    public override string Name => "DosBox Staging";

    /// <inheritdoc />
    public override string ShortName => "DosBox";

    /// <inheritdoc />
    public override ImmutableHashSet<GameEnum> SupportedGames { get; } =
    [
        GameEnum.Blood,
        GameEnum.Duke3D,
        GameEnum.Wang,
        //GameEnum.Slave,
        GameEnum.Redneck,
        GameEnum.RidesAgain
        //GameEnum.NAM,
        //GameEnum.Witchaven,
        //GameEnum.Witchaven2,
        //GameEnum.TekWar
    ];

    /// <inheritdoc />
    public override ImmutableHashSet<string> SupportedGamesVersions { get; } =
    [
        nameof(DukeVersionEnum.Duke3D_13D),
        nameof(DukeVersionEnum.Duke3D_Atomic)
    ];

    /// <inheritdoc />
    public override ImmutableHashSet<FeatureEnum> SupportedFeatures { get; } = [];

    /// <inheritdoc />
    protected override string ConfigFile => string.Empty;

    /// <inheritdoc />
    public override PortCmdArguments CmdArguments => new()
    {
        AddDirectory = null,
        MainGrp = null,
        AddGrp = null,
        AddFile = null,
        AddDef = null,
        AddCon = null,
        MainDef = null,
        MainCon = null,
        SkillLevel = null,
        AddGameDir = null,
        AddRff = null,
        AddSnd = null,
        SkipIntro = null,
        SkipStartup = null,
        SkipSteam = null
    };


    /// <inheritdoc />
    public override void BeforeStart(BaseGame game, BaseAddon campaign)
    {
        SaveFilesHelper.MoveSaveFilesFromStorage(
            GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id),
            GetGameSaveFilesFolder(game, campaign));
        FilesHelpers.RestoreRoute66Files(game);

        try
        {
            var config = Path.Combine(InstallFolderPath, "dosbox-staging.conf");

            if (File.Exists(config))
            {
                var file = File.ReadAllLines(config);

                for (var i = 0; i < file.Length; i++)
                {
                    if (file[i].StartsWith("memsize", StringComparison.OrdinalIgnoreCase) &&
                        !file[i].Trim().EndsWith("64", StringComparison.OrdinalIgnoreCase))
                    {
                        file[i] = "memsize = 64";
                        File.WriteAllLines(config, file);

                        break;
                    }
                }
            }
            else
            {
                File.WriteAllText(
                    config,
                    """
                    [dosbox]
                    memsize = 64

                    """
                    );
            }
        }
        catch (Exception ex)
        {
            if (_logger is not null)
            {
                _logger.LogWarning(ex, "Failed to write dosbox config");
            }
        }
    }

    /// <inheritdoc />
    public override void AfterEnd(BaseGame game, BaseAddon campaign)
    {
        SaveFilesHelper.MoveSaveFilesToStorage(
            GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id),
            GetGameSaveFilesFolder(game, campaign));
    }

    /// <inheritdoc />
    protected override void CustomModifyArgs(CmdParametersBuilder sb, BaseGame game, BaseAddon addon)
    {
        _ = sb.Append(" --noconsole");
        _ = sb.Append(@" -c ""cycles max""");
        _ = sb.Append(@" -c ""core dynamic""");
        _ = sb.Append(" -c \"exit\"");
    }
}
