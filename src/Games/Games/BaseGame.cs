using Common.Client.Helpers;
using Common;
using Common.Enums;
using Common.Interfaces;

namespace Games.Games;

/// <summary>
/// Base class that encapsulates logic for working with games and their mods
/// </summary>
public abstract class BaseGame : IGame
{
    /// <inheritdoc/>
    public string? GameInstallFolder { get; set; }

    /// <inheritdoc/>
    public bool IsBaseGameInstalled => IsInstalled(RequiredFiles);

    /// <inheritdoc/>
    public string CampaignsFolderPath => Path.Combine(ClientProperties.DataFolderPath, ShortName, "Campaigns");

    /// <inheritdoc/>
    public string MapsFolderPath => Path.Combine(ClientProperties.DataFolderPath, ShortName, "Maps");

    /// <inheritdoc/>
    public string ModsFolderPath => Path.Combine(ClientProperties.DataFolderPath, ShortName, "Mods");

    /// <inheritdoc/>
    public string SpecialFolderPath => Path.Combine(ClientProperties.DataFolderPath, ShortName, "Special");


    /// <inheritdoc/>
    public abstract GameEnum GameEnum { get; }

    /// <inheritdoc/>
    public abstract string FullName { get; }

    /// <inheritdoc/>
    public abstract string ShortName { get; }

    /// <inheritdoc/>
    public abstract List<string> RequiredFiles { get; }


    public BaseGame()
    {
        if (!Directory.Exists(CampaignsFolderPath))
        {
            Directory.CreateDirectory(CampaignsFolderPath);
        }

        if (!Directory.Exists(MapsFolderPath))
        {
            Directory.CreateDirectory(MapsFolderPath);
        }

        if (!Directory.Exists(ModsFolderPath))
        {
            Directory.CreateDirectory(ModsFolderPath);
        }

        if (!Directory.Exists(SpecialFolderPath))
        {
            Directory.CreateDirectory(SpecialFolderPath);
        }
    }


    /// <inheritdoc/>
    public abstract Dictionary<AddonVersion, IAddon> GetOriginalCampaigns();


    /// <summary>
    /// Do provided files exist in the game install folder
    /// </summary>
    /// <param name="files">List of required files</param>
    protected bool IsInstalled(List<string> files, string? path = null)
    {
        var gamePath = path is null ? GameInstallFolder : path;

        if (gamePath is null)
        {
            return false;
        }

        foreach (var file in files)
        {
            if (!File.Exists(Path.Combine(gamePath, file)))
            {
                return false;
            }
        }

        return true;
    }


    /// <summary>
    /// Does the file exist in the game install folder
    /// </summary>
    /// <param name="file">File</param>
    protected bool IsInstalled(string file)
    {
        if (GameInstallFolder is null)
        {
            return false;
        }

        if (!File.Exists(Path.Combine(GameInstallFolder, file)) &&
            !File.Exists(Path.Combine(GameInstallFolder, "addons", file)))
        {
            return false;
        }

        return true;
    }
}
