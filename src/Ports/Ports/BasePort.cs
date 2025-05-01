using Addons.Addons;
using Addons.Helpers;
using Common;
using Common.Client.Helpers;
using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Common.Serializable.Addon;
using CommunityToolkit.Diagnostics;
using Games.Games;
using System.Reflection;
using System.Text;

namespace Ports.Ports;

/// <summary>
/// Base class for ports
/// </summary>
public abstract class BasePort
{
    /// <summary>
    /// Port enum
    /// </summary>
    public abstract PortEnum PortEnum { get; }

    /// <summary>
    /// Main executable
    /// </summary>
    public string Exe
    {
        get
        {
            return CommonProperties.OSEnum switch
            {
                OSEnum.Windows => WinExe,
                OSEnum.Linux => LinExe,
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<string>(CommonProperties.OSEnum.ToString())
            };
        }
    }

    /// <summary>
    /// Windows executable
    /// </summary>
    protected abstract string WinExe { get; }

    /// <summary>
    /// Linux executable
    /// </summary>
    protected abstract string LinExe { get; }

    /// <summary>
    /// Name of the port
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Games supported by the port
    /// </summary>
    public abstract List<GameEnum> SupportedGames { get; }

    /// <summary>
    /// Features supported by the port
    /// </summary>
    public abstract List<FeatureEnum> SupportedFeatures { get; }

    /// <summary>
    /// Currently installed version
    /// </summary>
    public abstract string? InstalledVersion { get; }

    /// <summary>
    /// Path to port install folder
    /// </summary>
    public virtual string PortInstallFolderPath => Path.Combine(ClientProperties.PortsFolderPath, PortFolderName);

    /// <summary>
    /// Path to port saved games folder
    /// </summary>
    public virtual string PortSavedGamesFolderPath => Path.Combine(ClientProperties.SavedGamesFolderPath, Name);

    /// <summary>
    /// Is port installed
    /// </summary>
    public virtual bool IsInstalled => InstalledVersion is not null;

    /// <summary>
    /// Games versions supported by the port
    /// </summary>
    public virtual List<string> SupportedGamesVersions => [];

    /// <summary>
    /// Path to port exe
    /// </summary>
    public string PortExeFilePath => Path.Combine(PortInstallFolderPath, Exe);


    /// <summary>
    /// Name of the config file
    /// </summary>
    protected abstract string ConfigFile { get; }

    /// <summary>
    /// Cmd parameter to add folder to search path
    /// </summary>
    protected abstract string AddDirectoryParam { get; }

    /// <summary>
    /// Cmd parameter to load main GRP file
    /// </summary>
    protected abstract string MainGrpParam { get; }

    /// <summary>
    /// Cmd parameter to load additional GRP file
    /// </summary>
    protected abstract string AddGrpParam { get; }

    /// <summary>
    /// Cmd parameter to load additional file
    /// </summary>
    protected abstract string AddFileParam { get; }

    /// <summary>
    /// Cmd parameter to load additional Def file
    /// </summary>
    protected abstract string AddDefParam { get; }

    /// <summary>
    /// Cmd parameter to load additional Con file
    /// </summary>
    protected abstract string AddConParam { get; }

    /// <summary>
    /// Cmd parameter to load main Def file
    /// </summary>
    protected abstract string MainDefParam { get; }

    /// <summary>
    /// Cmd parameter to load main Con file
    /// </summary>
    protected abstract string MainConParam { get; }

    /// <summary>
    /// Cmd parameter for skill selection
    /// </summary>
    protected abstract string SkillParam { get; }

    /// <summary>
    /// Cmd parameter for setting game directory
    /// </summary>
    protected abstract string AddGameDirParam { get; }

    /// <summary>
    /// Cmd parameter for adding main rff file
    /// </summary>
    protected abstract string AddRffParam { get; }

    /// <summary>
    /// Cmd parameter for adding sound rff file
    /// </summary>
    protected abstract string AddSndParam { get; }

    /// <summary>
    /// Extension of the save game file
    /// </summary>
    protected IEnumerable<string> SaveFileExtensions => [".sav", ".esv"];

    /// <summary>
    /// Name of the folder that contains the port files
    /// By default is the same as <see cref="Name"/>
    /// </summary>
    private string PortFolderName => Name;

    /// <summary>
    /// Port's icon
    /// </summary>
    public Stream Icon => ImageHelper.FileNameToStream($"{Name}.png", Assembly.GetExecutingAssembly());


    protected BasePort()
    {
        if (!Directory.Exists(PortSavedGamesFolderPath))
        {
            _ = Directory.CreateDirectory(PortSavedGamesFolderPath);
        }
    }


    /// <summary>
    /// Get path to addon's saved games folder
    /// </summary>
    /// <param name="subFolder">Subfolder under port's saves folder</param>
    /// <param name="addonId">Addon Id</param>
    /// <returns></returns>
    public string GetPathToAddonSavedGamesFolder(string subFolder, string addonId)
    {
        var folderName = addonId;

        foreach (var ch in Path.GetInvalidFileNameChars())
        {
            folderName = folderName.Replace(ch, '_');
        }

        var result = Path.Combine(PortSavedGamesFolderPath, subFolder, folderName);

        return result;
    }


    /// <summary>
    /// Get command line parameters to start the game with selected campaign and autoload mods
    /// </summary>
    /// <param name="game"></param>
    /// <param name="addon"></param>
    /// <param name="mods"></param>
    /// <param name="skipIntro"></param>
    /// <param name="skipStartup"></param>
    /// <param name="skill"></param>
    public string GetStartGameArgs(
        IGame game,
        IAddon addon,
        Dictionary<AddonVersion, IAddon> mods,
        bool skipIntro,
        bool skipStartup,
        byte? skill = null
        )
    {
        StringBuilder sb = new();

        if (skipIntro)
        {
            GetSkipIntroParameter(sb);
        }

        if (skipStartup)
        {
            GetSkipStartupParameter(sb);
        }

        GetAutoloadModsArgs(sb, game, addon, mods);

        GetStartCampaignArgs(sb, game, addon);

        if (skill is not null)
        {
            _ = sb.Append($" {SkillParam}{skill}");
        }

        return sb.ToString();
    }


    /// <summary>
    /// Get startup args for manifested maps
    /// </summary>
    protected void GetMapArgs(StringBuilder sb, IAddon camp)
    {
        //TODO e#m#
        if (camp.StartMap is MapFileDto mapFile)
        {
            _ = sb.Append($@" {AddFileParam}""{camp.PathToFile}""");
            _ = sb.Append($@" -map ""{mapFile.File}""");
        }
        else
        {
            ThrowHelper.ThrowNotSupportedException();
        }
    }


    /// <summary>
    /// Get startup args for loose maps
    /// </summary>
    protected void GetLooseMapArgs(StringBuilder sb, IGame game, IAddon camp)
    {
        camp.StartMap.ThrowIfNotType<MapFileDto>(out var mapFile);

        _ = sb.Append($@" {AddDirectoryParam}""{game.MapsFolderPath}""");
        _ = sb.Append($@" -map ""{mapFile.File}""");
    }

    protected void GetBloodArgs(StringBuilder sb, BloodGame game, IAddon addon)
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


        addon.ThrowIfNotType<BloodCampaign>(out var bCamp);

        if (bCamp.INI is not null)
        {
            _ = sb.Append($@" -ini ""{bCamp.INI}""");
        }
        else if (bCamp.DependentAddons?.ContainsKey(nameof(BloodAddonEnum.BloodCP)) is true)
        {
            _ = sb.Append($@" -ini ""{ClientConsts.CrypticIni}""");
        }


        if (bCamp.FileName is null)
        {
            return;
        }


        if (bCamp.Type is AddonTypeEnum.TC)
        {
            if (bCamp.Executables is not null)
            {
                //don't add addon dir if the port is overridden
            }
            else if (bCamp.FileName.Equals("addon.json"))
            {
                _ = sb.Append($@" {AddGameDirParam}""{Path.GetDirectoryName(bCamp.PathToFile)}""");
            }
            else
            {
                _ = sb.Append($@" {AddFileParam}""{bCamp.PathToFile}""");
            }
        }
        else if (bCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, bCamp);
        }
        else
        {
            ThrowHelper.ThrowNotSupportedException($"Mod type {bCamp.Type} is not supported");
            return;
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

    protected void GetSlaveArgs(StringBuilder sb, SlaveGame game, IAddon addon)
    {
        if (addon is LooseMap)
        {
            GetLooseMapArgs(sb, game, addon);
            return;
        }


        addon.ThrowIfNotType<SlaveCampaign>(out var sCamp);

        if (sCamp.FileName is null)
        {
            return;
        }

        if (sCamp.Type is AddonTypeEnum.TC)
        {
            _ = sb.Append($@" {AddFileParam}""{sCamp.PathToFile}""");
        }
        else if (sCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, sCamp);
        }
        else
        {
            ThrowHelper.ThrowNotSupportedException($"Mod type {sCamp.Type} is not supported");
            return;
        }
    }

    protected void GetNamWW2GIArgs(StringBuilder sb, IGame game, IAddon addon)
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
            ThrowHelper.ThrowNotSupportedException();
        }


        if (addon is LooseMap)
        {
            GetLooseMapArgs(sb, game, addon);
            return;
        }


        addon.ThrowIfNotType<DukeCampaign>(out var dCamp);

        if (addon.Id.Equals(nameof(WW2GIAddonEnum.Platoon).ToLower()))
        {
            _ = sb.Append($" {AddGrpParam}PLATOONL.DAT {MainConParam}PLATOONL.DEF");
        }
        else if (dCamp.MainCon is null)
        {
            _ = sb.Append($" {MainConParam}GAME.CON");
        }


        if (dCamp.FileName is null)
        {
            return;
        }


        if (dCamp.MainCon is not null)
        {
            _ = sb.Append($@" {MainConParam}""{dCamp.MainCon}""");
        }

        if (dCamp.AdditionalCons?.Count > 0)
        {
            foreach (var con in dCamp.AdditionalCons)
            {
                _ = sb.Append($@" {AddConParam}""{con}""");
            }
        }


        if (dCamp.Type is AddonTypeEnum.TC)
        {
            _ = sb.Append($@" {AddFileParam}""{dCamp.PathToFile}""");
        }
        else if (dCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, dCamp);
        }
        else
        {
            ThrowHelper.ThrowNotSupportedException($"Mod type {dCamp.Type} is not supported");
            return;
        }
    }

    /// <summary>
    /// Get command line arguments to load mods
    /// </summary>
    /// <param name="sb">String builder for parameters</param>
    /// <param name="game">Game</param>
    /// <param name="addon">Campaign\map</param>
    /// <param name="mods">Autoload mods</param>
    protected virtual void GetAutoloadModsArgs(StringBuilder sb, IGame game, IAddon addon, Dictionary<AddonVersion, IAddon> mods)
    {
        if (mods.Count == 0)
        {
            return;
        }

        var enabledModsCount = 0;
        HashSet<string> addedModsFiles = [];

        foreach (var mod in mods)
        {
            if (mod.Value is not AutoloadMod aMod)
            {
                continue;
            }

            if (!AutoloadModsValidator.ValidateAutoloadMod(aMod, addon, mods, SupportedFeatures))
            {
                continue;
            }

            if (!addedModsFiles.TryGetValue(mod.Value.FileName!, out _))
            {
                _ = addedModsFiles.Add(mod.Value.FileName!);
                _ = sb.Append($@" {AddFileParam}""{aMod.FileName}""");
            }

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
    /// Method to perform after port is finished
    /// </summary>
    /// <param name="game">Game</param>
    /// <param name="campaign">Campaign</param>
    public abstract void AfterEnd(IGame game, IAddon campaign);

    /// <summary>
    /// Method to perform before starting the port
    /// </summary>
    /// <param name="game">Game</param>
    /// <param name="campaign">Campaign</param>
    public abstract void BeforeStart(IGame game, IAddon campaign);

    /// <summary>
    /// Get command line arguments to start custom map or campaign
    /// </summary>
    /// <param name="sb">String builder for parameters</param>
    /// <param name="game">Game</param>
    /// <param name="addon">Map/campaign</param>
    protected abstract void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon);

    /// <summary>
    /// Return command line parameter to skip intro
    /// </summary>
    /// <param name="sb">String builder for parameters</param>
    protected abstract void GetSkipIntroParameter(StringBuilder sb);

    /// <summary>
    /// Return command line parameter to skip startup window
    /// </summary>
    /// <param name="sb">String builder for parameters</param>
    protected abstract void GetSkipStartupParameter(StringBuilder sb);



    /// <summary>
    /// Remove route 66 art files overrides used for RedNukem
    /// </summary>
    protected void RestoreRoute66Files(IGame game)
    {
        if (game is not RedneckGame)
        {
            return;
        }

        Guard.IsNotNull(game.GameInstallFolder);

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
    /// Restore WT's ART files
    /// </summary>
    protected void RestoreWtFiles(IGame game)
    {
        if (game is not DukeGame dGame)
        {
            return;
        }

        Guard.IsNotNull(game.GameInstallFolder);

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
}
