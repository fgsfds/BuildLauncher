using Common.All.Enums;
using Common.All.Interfaces;
using Common.Client.Helpers;

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
    public string CampaignsFolderPath => Path.Combine(ClientProperties.DataFolderPath, "Addons", ShortName, "Campaigns");

    /// <inheritdoc/>
    public string MapsFolderPath => Path.Combine(ClientProperties.DataFolderPath, "Addons", ShortName, "Maps");

    /// <inheritdoc/>
    public string ModsFolderPath => Path.Combine(ClientProperties.DataFolderPath, "Addons", ShortName, "Mods");


    /// <inheritdoc/>
    public abstract GameEnum GameEnum { get; }

    /// <inheritdoc/>
    public abstract string FullName { get; }

    /// <inheritdoc/>
    public abstract string ShortName { get; }

    /// <inheritdoc/>
    public abstract List<string> RequiredFiles { get; }

    /// <inheritdoc/>
    public abstract Enum? Skills { get; }


    protected BaseGame()
    {
        try
        {
            MoveOldFolders();
        }
        catch
        {
            Directory.Delete(Path.Combine(ClientProperties.DataFolderPath, ShortName));
        }

        if (!Directory.Exists(CampaignsFolderPath))
        {
            _ = Directory.CreateDirectory(CampaignsFolderPath);
        }

        if (!Directory.Exists(MapsFolderPath))
        {
            _ = Directory.CreateDirectory(MapsFolderPath);
        }

        if (!Directory.Exists(ModsFolderPath))
        {
            _ = Directory.CreateDirectory(ModsFolderPath);
        }

        DeleteSpecial();
    }

    [Obsolete]
    private void DeleteSpecial()
    {
        if (Directory.Exists(Path.Combine(ClientProperties.DataFolderPath, "Addons", ShortName, "Special")))
        {
            Directory.Delete(Path.Combine(ClientProperties.DataFolderPath, "Addons", ShortName, "Special"), true);
        }
    }

    [Obsolete]
    private void MoveOldFolders()
    {
        var newGameDataFolder = Path.Combine(ClientProperties.DataFolderPath, "Addons", ShortName);
        if (!Directory.Exists(newGameDataFolder))
        {
            _ = Directory.CreateDirectory(newGameDataFolder);
        }

        var oldCampsFolder = Path.Combine(ClientProperties.DataFolderPath, ShortName, "Campaigns");
        if (Directory.Exists(oldCampsFolder))
        {
            Directory.Move(oldCampsFolder, CampaignsFolderPath);
        }

        var oldMapsFolder = Path.Combine(ClientProperties.DataFolderPath, ShortName, "Maps");
        if (Directory.Exists(oldMapsFolder))
        {
            Directory.Move(oldMapsFolder, MapsFolderPath);
        }

        var oldModsFolder = Path.Combine(ClientProperties.DataFolderPath, ShortName, "Mods");
        if (Directory.Exists(oldModsFolder))
        {
            Directory.Move(oldModsFolder, ModsFolderPath);
        }

        var oldGameDataDirectory = Path.Combine(ClientProperties.DataFolderPath, ShortName);
        if (Directory.Exists(oldGameDataDirectory))
        {
            Directory.Delete(oldGameDataDirectory, true);
        }
    }


    /// <summary>
    /// Do provided files exist in the folder
    /// </summary>
    /// <param name="files">List of required files</param>
    /// <param name="path">Folder where the files are searched</param>
    protected bool IsInstalled(List<string> files, string? path = null)
    {
        var gamePath = path ?? GameInstallFolder;

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
}
