using System.Buffers;
using System.Collections.Immutable;
using Addons.Addons;
using Core.All.Enums;
using Core.All.Helpers;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Games.Games;
using Ports.Builders;

namespace Ports.Ports;

/// <summary>
///     Base class for ports.
/// </summary>
public abstract class BasePort : IInstallable
{
    private static readonly SearchValues<char> InvalidChars =
        SearchValues.Create(Path.GetInvalidFileNameChars());

    /// <summary>
    ///     Port enum.
    /// </summary>
    public abstract PortEnum PortEnum { get; }

    /// <summary>
    ///     Main executable.
    /// </summary>
    public string Exe
    {
        get
        {
            return CommonProperties.OSEnum switch
            {
                OSEnum.Windows => WinExe,
                OSEnum.Linux => LinExe,
                _ => throw new ArgumentOutOfRangeException(nameof(CommonProperties.OSEnum), CommonProperties.OSEnum, $"Unsupported OS: {CommonProperties.OSEnum}.")
            };
        }
    }

    /// <summary>
    ///     Windows executable.
    /// </summary>
    protected abstract string WinExe { get; }

    /// <summary>
    ///     Linux executable.
    /// </summary>
    protected abstract string LinExe { get; }

    /// <summary>
    ///     Name of the port.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    ///     Name of the folder that contains the port files.
    ///     By default is the same as <see cref="Name" />.
    /// </summary>
    public virtual string ShortName => Name;

    /// <summary>
    ///     Games supported by the port.
    /// </summary>
    public abstract ImmutableHashSet<GameEnum> SupportedGames { get; }

    /// <summary>
    ///     Features supported by the port.
    /// </summary>
    public abstract ImmutableHashSet<FeatureEnum> SupportedFeatures { get; }

    /// <summary>
    ///     Path to port saved games folder.
    /// </summary>
    public string PortSavedGamesFolderPath => Path.Combine(ClientProperties.SavedGamesFolderPath, Name);

    /// <summary>
    ///     Game versions supported by the port.
    /// </summary>
    public virtual ImmutableHashSet<string> SupportedGamesVersions { get; } = [];

    /// <summary>
    ///     Path to port executable.
    /// </summary>
    public string PortExeFilePath => Path.Combine(InstallFolderPath, Exe);

    /// <summary>
    ///     Name of the config file.
    /// </summary>
    protected abstract string ConfigFile { get; }

    /// <summary>
    ///     Path to the port config file.
    /// </summary>
    public string PortConfigFilePath => Path.Combine(InstallFolderPath, ConfigFile);

    /// <summary>
    ///     Command-line parameters for building arguments.
    /// </summary>
    public abstract PortCmdArguments CmdArguments { get; }

    /// <summary>
    ///     Port's icon.
    /// </summary>
    public long IconId => PortEnum.GetUniqueHash();

    /// <summary>
    ///     Can this port be downloaded.
    /// </summary>
    public virtual bool IsDownloadable => true;

    /// <summary>
    ///     Indicates whether skill level can be selected from the command line.
    /// </summary>
    public bool IsSkillSelectionAvailable => CmdArguments.SkillLevel is not null;

    /// <inheritdoc />
    public virtual string? InstalledVersion
    {
        get
        {
            var versionFile = Path.Combine(InstallFolderPath, "version");

            if (!File.Exists(versionFile))
            {
                return null;
            }

            try
            {
                return File.ReadAllText(versionFile);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <inheritdoc />
    public virtual string InstallFolderPath => Path.Combine(ClientProperties.PortsFolderPath, ShortName);

    /// <inheritdoc />
    public virtual bool IsInstalled => InstalledVersion is not null;

    /// <summary>
    ///     Gets the path to an addon's saved games folder.
    /// </summary>
    /// <param name="subFolder">
    ///     Subfolder under port's saves folder
    /// </param>
    /// <param name="addonId">
    ///     Addon Id
    /// </param>
    protected string GetPathToAddonSavedGamesFolder(string subFolder, string addonId)
    {
        var folderName = string.Create(
            addonId.Length, addonId, (span, state) =>
            {
                state.AsSpan().CopyTo(span);

                for (var i = 0; i < span.Length; i++)
                {
                    if (InvalidChars.Contains(span[i]))
                    {
                        span[i] = '_';
                    }
                }
            }
            );

        return Path.Combine(PortSavedGamesFolderPath, subFolder, folderName);
    }

    /// <summary>
    ///     Gets the command-line arguments to start the game with the selected campaign and autoload mods.
    /// </summary>
    /// <param name="game">
    ///     Game to start
    /// </param>
    /// <param name="addon">
    ///     Addon to start
    /// </param>
    /// <param name="mods">
    ///     Autoload mods
    /// </param>
    /// <param name="enabledOptions">
    ///     List of enabled options
    /// </param>
    /// <param name="skipIntro">
    ///     Skip intro
    /// </param>
    /// <param name="skipStartup">
    ///     Skip startup window
    /// </param>
    /// <param name="skill">
    ///     Skill level
    /// </param>
    public string GetStartGameArgs(
        BaseGame game,
        BaseAddon addon,
        IReadOnlyList<BaseAddon> mods,
        IReadOnlyList<string> enabledOptions,
        bool skipIntro,
        bool skipStartup,
        byte? skill = null
        )
    {
        var sb = CmdParametersBuilderFactory.Create(game, addon, this);

        sb.AppendAutoloadModsArgs(mods);

        if (addon is LooseMap)
        {
            sb.AppendLooseMapArgs();
        }
        else
        {
            sb.AppendGameArgs(game, addon);
        }

        sb.AppendOptionsArgs(enabledOptions);

        if (skill is not null)
        {
            _ = sb.Append($" {CmdArguments.SkillLevel}{skill}");
        }

        if (skipIntro)
        {
            sb.AppendSkipIntroParameter();
        }

        if (skipStartup)
        {
            sb.AppendSkipStartupParameter();
        }

        CustomModifyArgs(sb, game, addon);

        if (game is not FuryGame && addon.MainDef is null)
        {
            sb.AppendOverrideMainDef();
        }
        else
        {
            sb.AppendMainDefArgs();
        }

        sb.AppendAdditionalDefsArgs();

        return sb.ToString();
    }

    /// <summary>
    ///     Allows derived ports to modify command-line arguments specific to their implementation.
    /// </summary>
    /// <param name="sb">
    ///     A builder object used to construct command-line arguments.
    /// </param>
    /// <param name="game">
    ///     The base game for which the command-line arguments are being built.
    /// </param>
    /// <param name="addon">
    ///     The addon or modification being applied to the game.
    /// </param>
    protected virtual void CustomModifyArgs(CmdParametersBuilder sb, BaseGame game, BaseAddon addon) { }

    /// <summary>
    ///     Performs cleanup after the port exits.
    /// </summary>
    /// <param name="game">
    ///     Game
    /// </param>
    /// <param name="campaign">
    ///     Campaign
    /// </param>
    public abstract void AfterEnd(BaseGame game, BaseAddon campaign);

    /// <summary>
    ///     Performs setup before starting the port.
    /// </summary>
    /// <param name="game">
    ///     Game
    /// </param>
    /// <param name="campaign">
    ///     Campaign
    /// </param>
    public abstract void BeforeStart(BaseGame game, BaseAddon campaign);

    /// <summary>
    ///     Gets the folder where the game stores its save files for the given campaign.
    /// </summary>
    /// <param name="game">
    ///     Game.
    /// </param>
    /// <param name="campaign">
    ///     Campaign.
    /// </param>
    protected virtual string GetGameSaveFilesFolder(BaseGame game, BaseAddon campaign)
    {
        return game.GameInstallFolder ?? throw new InvalidOperationException(nameof(game.GameInstallFolder));
    }
}
