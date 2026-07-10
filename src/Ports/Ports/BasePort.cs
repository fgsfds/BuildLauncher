using System.Collections.Immutable;
using System.Text;
using Addons.Addons;
using Addons.Helpers;
using Core.All.Enums;
using Core.All.Enums.Addons;
using Core.All.Helpers;
using Core.All.Serializable.Addon;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Games.Games;

namespace Ports.Ports;

/// <summary>
///     Base class for ports.
/// </summary>
public abstract class BasePort : IInstallable
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BasePort" /> class.
    /// </summary>
    protected BasePort()
    {
        if (!Directory.Exists(PortSavedGamesFolderPath))
        {
            _ = Directory.CreateDirectory(PortSavedGamesFolderPath);
        }
    }

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
    public abstract List<GameEnum> SupportedGames { get; }

    /// <summary>
    ///     Features supported by the port.
    /// </summary>
    public abstract List<FeatureEnum> SupportedFeatures { get; }

    /// <summary>
    ///     Path to port saved games folder.
    /// </summary>
    public virtual string PortSavedGamesFolderPath => Path.Combine(ClientProperties.SavedGamesFolderPath, Name);

    /// <summary>
    ///     Game versions supported by the port.
    /// </summary>
    public virtual List<string> SupportedGamesVersions => [];

    /// <summary>
    ///     Path to port executable.
    /// </summary>
    public string PortExeFilePath => Path.Combine(InstallFolderPath, Exe);

    /// <summary>
    ///     Name of the config file.
    /// </summary>
    protected abstract string ConfigFile { get; }

    /// <summary>
    ///     Command-line parameter to add folder to search path.
    /// </summary>
    protected abstract string AddDirectoryParam { get; }

    /// <summary>
    ///     Command-line parameter to load main GRP file.
    /// </summary>
    protected abstract string MainGrpParam { get; }

    /// <summary>
    ///     Command-line parameter to load additional GRP file.
    /// </summary>
    protected abstract string AddGrpParam { get; }

    /// <summary>
    ///     Command-line parameter to load additional file.
    /// </summary>
    protected abstract string AddFileParam { get; }

    /// <summary>
    ///     Command-line parameter to load additional Def file.
    /// </summary>
    protected abstract string AddDefParam { get; }

    /// <summary>
    ///     Command-line parameter to load additional Con file.
    /// </summary>
    protected abstract string AddConParam { get; }

    /// <summary>
    ///     Command-line parameter to load main Def file.
    /// </summary>
    protected abstract string MainDefParam { get; }

    /// <summary>
    ///     Command-line parameter to load main Con file.
    /// </summary>
    protected abstract string MainConParam { get; }

    /// <summary>
    ///     Command-line parameter for skill selection.
    /// </summary>
    protected abstract string SkillParam { get; }

    /// <summary>
    ///     Command-line parameter for setting game directory.
    /// </summary>
    protected abstract string AddGameDirParam { get; }

    /// <summary>
    ///     Command-line parameter for adding main RFF file.
    /// </summary>
    protected abstract string AddRffParam { get; }

    /// <summary>
    ///     Command-line parameter for adding sound RFF file.
    /// </summary>
    protected abstract string AddSndParam { get; }

    /// <summary>
    ///     Extensions of save game files.
    /// </summary>
    protected ImmutableArray<string> SaveFileExtensions =>
    [
        ".sav",
        ".esv"
    ];

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
    public abstract bool IsSkillSelectionAvailable { get; }

    /// <inheritdoc />
    public abstract string? InstalledVersion { get; }

    /// <inheritdoc />
    public virtual string InstallFolderPath => Path.Combine(ClientProperties.PortsFolderPath, ShortName);

    /// <inheritdoc />
    public virtual bool IsInstalled => InstalledVersion is not null;

    /// <summary>
    ///     Gets the path to an addon's saved games folder.
    /// </summary>
    /// <param name="subFolder">Subfolder under port's saves folder</param>
    /// <param name="addonId">Addon Id</param>
    protected string GetPathToAddonSavedGamesFolder(string subFolder, string addonId)
    {
        var folderName = addonId;

        foreach (var ch in Path.GetInvalidFileNameChars())
        {
            folderName = folderName.Replace(ch, '_');
        }

        return Path.Combine(PortSavedGamesFolderPath, subFolder, folderName);
    }

    /// <summary>
    ///     Gets the command-line arguments to start the game with the selected campaign and autoload mods.
    /// </summary>
    /// <param name="game">Game to start</param>
    /// <param name="addon">Addon to start</param>
    /// <param name="mods">Autoload mods</param>
    /// <param name="enabledOptions">List of enabled options</param>
    /// <param name="skipIntro">Skip intro</param>
    /// <param name="skipStartup">Skip startup window</param>
    /// <param name="skill">Skill level</param>
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
        StringBuilder sb = new();

        GetAutoloadModsArgs(sb, game, addon, mods);

        GetStartCampaignArgs(sb, game, addon);

        if (addon.Options is not null && enabledOptions.Any())
        {
            GetOptionsArgs(sb, game, addon, enabledOptions);
        }

        if (skill is not null)
        {
            _ = sb.Append($" {SkillParam}{skill}");
        }

        if (skipIntro)
        {
            GetSkipIntroParameter(sb);
        }

        if (skipStartup)
        {
            GetSkipStartupParameter(sb);
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Appends command-line arguments for enabled options.
    /// </summary>
    protected void GetOptionsArgs(
        StringBuilder sb,
        BaseGame game,
        BaseAddon addon,
        IReadOnlyList<string> enabledOptions
        )
    {
        ArgumentNullException.ThrowIfNull(addon.Options);

        foreach (var optionName in enabledOptions)
        {
            if (!addon.Options.TryGetValue(optionName, out var options))
            {
                throw new KeyNotFoundException($"Option '{optionName}' not found in addon options.");
            }

            foreach (var option in options)
            {
                if (option.Value is OptionalParameterTypeEnum.DEF)
                {
                    _ = sb.Append($@" {AddDefParam}""{option.Key}""");
                }
                else if (option.Value is OptionalParameterTypeEnum.INI &&
                         game is BloodGame)
                {
                    _ = sb.Append($@" -ini ""{option.Key}""");
                }
                else
                {
                    throw new NotSupportedException($"Option '{option.Key}' has unsupported type '{option.Value}' for non-Blood games.");
                }
            }
        }
    }

    /// <summary>
    ///     Gets startup arguments for manifested maps.
    /// </summary>
    protected void GetMapArgs(StringBuilder sb, BaseAddon camp)
    {
        if (camp.FileInfo is null)
        {
            throw new InvalidOperationException("Campaign file info is required for map args");
        }

        //TODO e#m#
        if (camp.StartMap is MapFileJsonModel mapFile)
        {
            _ = sb.Append($@" {AddFileParam}""{camp.FileInfo.PathToFile}""");
            _ = sb.Append($@" -map ""{mapFile.File}""");
        }
        else
        {
            throw new NotSupportedException($"Unsupported start map type: {camp.StartMap?.GetType().Name}.");
        }
    }

    /// <summary>
    ///     Gets startup arguments for loose maps.
    /// </summary>
    protected virtual void GetLooseMapArgs(StringBuilder sb, BaseGame game, BaseAddon camp)
    {
        if (camp.StartMap is not MapFileJsonModel mapFile)
        {
            throw new ArgumentException($"Expected {nameof(MapFileJsonModel)} start map but received {camp.StartMap?.GetType().Name}.", nameof(camp));
        }

        _ = sb.Append($@" {AddDirectoryParam}""{game.MapsFolderPath}""");
        _ = sb.Append($@" -map ""{mapFile.File}""");
    }

    /// <summary>
    ///     Appends command-line arguments for Blood game campaigns.
    /// </summary>
    protected virtual void GetBloodArgs(StringBuilder sb, BloodGame game, BaseAddon addon)
    {
        if (addon is LooseMap lMap)
        {
            if (lMap.BloodIni is null)
            {
                _ = sb.Append($@" -ini ""{ClientConsts.BloodIni}""");
            }
            else
            {
                _ = sb.Append($@" -ini ""{Path.GetFileName(lMap.BloodIni)}""");
            }

            GetLooseMapArgs(sb, game, addon);

            return;
        }

        if (addon is not BloodCampaign bCamp)
        {
            throw new ArgumentException($"Expected {nameof(BloodCampaign)} but received {addon.GetType().Name}.", nameof(addon));
        }

        if (bCamp.INI is not null)
        {
            _ = sb.Append($@" -ini ""{bCamp.INI}""");
        }
        else if (bCamp.DependentAddons?.ContainsKey(nameof(BloodAddonEnum.BloodCP)) is true)
        {
            _ = sb.Append($@" -ini ""{ClientConsts.CrypticIni}""");
        }

        if (bCamp.FileInfo is null)
        {
            return;
        }

        if (bCamp.Type is AddonTypeEnum.TC)
        {
            if (bCamp.Executables is not null)
            {
                //don't add addon dir if the port is overridden
            }
            else if (bCamp.FileInfo.IsFolder)
            {
                _ = sb.Append($@" {AddGameDirParam}""{bCamp.FileInfo.PathToFolder}""");
            }
            else
            {
                _ = sb.Append($@" {AddFileParam}""{bCamp.FileInfo.PathToFile}""");
            }
        }
        else if (bCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, bCamp);
        }
        else
        {
            throw new NotSupportedException($"Mod type {bCamp.Type} is not supported");
        }

        if (bCamp.RFF is not null)
        {
            _ = sb.Append($@" {AddRffParam}""{bCamp.RFF}""");
        }

        if (bCamp.SND is not null)
        {
            _ = sb.Append($@" {AddSndParam}""{bCamp.SND}""");
        }
    }

    /// <summary>
    ///     Appends command-line arguments for Slave (PowerSlave) game campaigns.
    /// </summary>
    protected void GetSlaveArgs(StringBuilder sb, SlaveGame game, BaseAddon addon)
    {
        if (addon is LooseMap)
        {
            GetLooseMapArgs(sb, game, addon);

            return;
        }

        if (addon is not GenericCampaign sCamp)
        {
            throw new ArgumentException($"Expected {nameof(GenericCampaign)} but received {addon.GetType().Name}.", nameof(addon));
        }

        if (sCamp.FileInfo is null)
        {
            return;
        }

        if (sCamp.Type is AddonTypeEnum.TC)
        {
            _ = sb.Append($@" {AddFileParam}""{sCamp.FileInfo.PathToFile}""");
        }
        else if (sCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, sCamp);
        }
        else
        {
            throw new NotSupportedException($"Mod type {sCamp.Type} is not supported");
        }
    }

    /// <summary>
    ///     Appends command-line arguments for NAM and WW2GI game campaigns.
    /// </summary>
    protected virtual void GetNamWW2GIArgs(StringBuilder sb, BaseGame game, BaseAddon addon)
    {
        if (game is NamGame)
        {
            _ = sb.Append($" -nam {MainGrpParam}NAM.GRP");
        }
        else if (game is WW2GIGame)
        {
            _ = sb.Append($" -ww2gi {MainGrpParam}WW2GI.GRP");
        }
        else
        {
            throw new NotSupportedException($"Unsupported game type {game.GetType().Name} for NAM/WW2GI arguments.");
        }

        if (addon is LooseMap)
        {
            GetLooseMapArgs(sb, game, addon);

            return;
        }

        if (addon is not DukeCampaign dCamp)
        {
            throw new ArgumentException($"Expected {nameof(DukeCampaign)} but received {addon.GetType().Name}.", nameof(addon));
        }

        if (addon.AddonId.Id.Equals(nameof(WW2GIAddonEnum.Platoon), StringComparison.OrdinalIgnoreCase))
        {
            _ = sb.Append($" {AddGrpParam}PLATOONL.DAT {MainConParam}PLATOONL.DEF");
        }
        else if (dCamp.MainCon is null)
        {
            _ = sb.Append($" {MainConParam}GAME.CON");
        }

        if (dCamp.FileInfo is null)
        {
            return;
        }

        if (dCamp.MainCon is not null)
        {
            _ = sb.Append($@" {MainConParam}""{dCamp.MainCon}""");
        }

        if (dCamp.AdditionalCons?.Any() is true)
        {
            foreach (var con in dCamp.AdditionalCons)
            {
                _ = sb.Append($@" {AddConParam}""{con}""");
            }
        }

        if (dCamp.Type is AddonTypeEnum.TC)
        {
            _ = sb.Append($@" {AddFileParam}""{dCamp.FileInfo.PathToFile}""");
        }
        else if (dCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, dCamp);
        }
        else
        {
            throw new NotSupportedException($"Mod type {dCamp.Type} is not supported");
        }
    }

    /// <summary>
    ///     Gets command-line arguments to load mods.
    /// </summary>
    /// <param name="sb">String builder for parameters</param>
    /// <param name="game">Game</param>
    /// <param name="addon">Campaign\map</param>
    /// <param name="mods">Autoload mods</param>
    protected virtual void GetAutoloadModsArgs(StringBuilder sb, BaseGame game, BaseAddon addon, IReadOnlyList<BaseAddon> mods)
    {
        if (mods.Count == 0)
        {
            return;
        }

        var enabledModsCount = 0;

        foreach (var mod in mods)
        {
            if (mod is not AutoloadMod aMod)
            {
                continue;
            }

            if (!AutoloadModsValidator.ValidateAutoloadMod(aMod, addon, mods, SupportedFeatures))
            {
                continue;
            }

            if (aMod.FileInfo is null)
            {
                continue;
            }

            if (aMod.FileInfo.IsFolder)
            {
                throw new InvalidOperationException("Folder mods are not supported in autoload");
            }

            _ = sb.Append($@" {AddFileParam}""{aMod.FileInfo.FileName}""");

            if (aMod.AdditionalDefs is not null)
            {
                foreach (var def in aMod.AdditionalDefs)
                {
                    _ = sb.Append($@" {AddDefParam}""{def}""");
                }
            }

            if (aMod.AdditionalCons is not null)
            {
                foreach (var con in aMod.AdditionalCons)
                {
                    _ = sb.Append($@" {AddConParam}""{con}""");
                }
            }

            enabledModsCount++;
        }

        if (enabledModsCount > 0 &&
            //Raze sets mods dir in the config
            PortEnum is not PortEnum.Raze)
        {
            _ = sb.Append($@" {AddDirectoryParam}""{game.ModsFolderPath}""");
        }
    }

    /// <summary>
    ///     Performs cleanup after the port exits.
    /// </summary>
    /// <param name="game">Game</param>
    /// <param name="campaign">Campaign</param>
    public abstract void AfterEnd(BaseGame game, BaseAddon campaign);

    /// <summary>
    ///     Performs setup before starting the port.
    /// </summary>
    /// <param name="game">Game</param>
    /// <param name="campaign">Campaign</param>
    public abstract void BeforeStart(BaseGame game, BaseAddon campaign);

    /// <summary>
    ///     Gets command-line arguments to start a custom map or campaign.
    /// </summary>
    /// <param name="sb">String builder for parameters</param>
    /// <param name="game">Game</param>
    /// <param name="addon">Map/campaign</param>
    protected abstract void GetStartCampaignArgs(StringBuilder sb, BaseGame game, BaseAddon addon);

    /// <summary>
    ///     Appends the command-line parameter to skip the intro.
    /// </summary>
    /// <param name="sb">String builder for parameters</param>
    protected abstract void GetSkipIntroParameter(StringBuilder sb);

    /// <summary>
    ///     Appends the command-line parameter to skip the startup window.
    /// </summary>
    /// <param name="sb">String builder for parameters</param>
    protected abstract void GetSkipStartupParameter(StringBuilder sb);

    /// <summary>
    ///     Removes Route 66 art file overrides used for RedNukem.
    /// </summary>
    protected void RestoreRoute66Files(BaseGame game)
    {
        if (game is not RedneckGame)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(game.GameInstallFolder);

        var tilesA2 = Path.Combine(game.GameInstallFolder, "TILES024.ART");
        var tilesB2 = Path.Combine(game.GameInstallFolder, "TILES025.ART");
        var turdMovAnm2 = Path.Combine(game.GameInstallFolder, "TURDMOV.ANM");
        var turdMovVoc2 = Path.Combine(game.GameInstallFolder, "TURDMOV.VOC");
        var endMovAnm2 = Path.Combine(game.GameInstallFolder, "RR_OUTRO.ANM");
        var endMovVoc2 = Path.Combine(game.GameInstallFolder, "LN_FINAL.VOC");

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

    /// <summary>
    ///     Restores Duke WT's ART files.
    /// </summary>
    protected void RestoreWtFiles(BaseGame game)
    {
        if (game is not DukeGame dGame)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(game.GameInstallFolder);

        var art1 = Path.Combine(game.GameInstallFolder, "TILES009.ART");
        var art1r = Path.Combine(game.GameInstallFolder, "TILES009._ART");

        var art2 = Path.Combine(game.GameInstallFolder, "TILES020.ART");
        var art2r = Path.Combine(game.GameInstallFolder, "TILES020._ART");

        var art3 = Path.Combine(game.GameInstallFolder, "TILES021.ART");
        var art3r = Path.Combine(game.GameInstallFolder, "TILES021._ART");

        var art4 = Path.Combine(game.GameInstallFolder, "TILES022.ART");
        var art4r = Path.Combine(game.GameInstallFolder, "TILES022._ART");

        if (File.Exists(art1r))
        {
            File.Move(art1r, art1, true);
        }

        if (File.Exists(art2r))
        {
            File.Move(art2r, art2, true);
        }

        if (File.Exists(art3r))
        {
            File.Move(art3r, art3, true);
        }

        if (File.Exists(art4r))
        {
            File.Move(art4r, art4, true);
        }

        if (dGame.DukeWTInstallPath is not null)
        {
            art1 = Path.Combine(dGame.DukeWTInstallPath, "TILES009.ART");
            art1r = Path.Combine(dGame.DukeWTInstallPath, "TILES009._ART");

            art2 = Path.Combine(dGame.DukeWTInstallPath, "TILES020.ART");
            art2r = Path.Combine(dGame.DukeWTInstallPath, "TILES020._ART");

            art3 = Path.Combine(dGame.DukeWTInstallPath, "TILES021.ART");
            art3r = Path.Combine(dGame.DukeWTInstallPath, "TILES021._ART");

            art4 = Path.Combine(dGame.DukeWTInstallPath, "TILES022.ART");
            art4r = Path.Combine(dGame.DukeWTInstallPath, "TILES022._ART");

            if (File.Exists(art1r))
            {
                File.Move(art1r, art1, true);
            }

            if (File.Exists(art2r))
            {
                File.Move(art2r, art2, true);
            }

            if (File.Exists(art3r))
            {
                File.Move(art3r, art3, true);
            }

            if (File.Exists(art4r))
            {
                File.Move(art4r, art4, true);
            }
        }
    }

    /// <summary>
    ///     Moves save files from the addon's saved games storage folder to the game's install folder.
    /// </summary>
    /// <param name="game">The game instance containing the target install folder.</param>
    /// <param name="campaign">The addon campaign whose saves are to be moved.</param>
    protected virtual void MoveSaveFilesFromStorage(BaseGame game, BaseAddon campaign)
    {
        var saveFolder = GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id);

        if (!Directory.Exists(saveFolder))
        {
            return;
        }

        var saves = Directory.GetFiles(saveFolder);

        if (game.GameInstallFolder is null)
        {
            return;
        }

        foreach (var save in saves)
        {
            var destFileName = Path.Combine(game.GameInstallFolder, Path.GetFileName(save));
            File.Move(save, destFileName, true);
        }
    }

    /// <summary>
    ///     Moves save files from the game installation folder to the addon's saved games folder.
    /// </summary>
    /// <param name="game">The game whose save files are to be moved.</param>
    /// <param name="campaign">The addon or campaign whose save folder is the destination.</param>
    protected virtual void MoveSaveFilesToStorage(BaseGame game, BaseAddon campaign)
    {
        var saveFolder = GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id);

        ArgumentNullException.ThrowIfNull(game.GameInstallFolder);
        var path = game.GameInstallFolder;

        var files = from file in Directory.GetFiles(path)
                    from ext in SaveFileExtensions
                    where file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)
                    select file;

        if (!Directory.Exists(saveFolder))
        {
            _ = Directory.CreateDirectory(saveFolder);
        }

        foreach (var file in files)
        {
            var destFileName = Path.Combine(saveFolder, Path.GetFileName(file));
            File.Move(file, destFileName, true);
        }
    }
}
