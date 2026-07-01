using System.Diagnostics.CodeAnalysis;
using Core.All.Enums;
using Core.Client.Helpers;

namespace Games.Games;

/// <summary>
///     Base class that encapsulates logic for working with games and their mods.
/// </summary>
public abstract class BaseGame
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BaseGame" /> class.
    /// </summary>
    protected BaseGame()
    {
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
    }

    /// <summary>
    ///     Game install folder.
    /// </summary>
    public string? GameInstallFolder { get; set; }

    /// <summary>
    ///     Is base game installed.
    /// </summary>
    public bool IsBaseGameInstalled => IsInstalled(RequiredFiles);

    /// <summary>
    ///     Path to custom campaigns folder.
    /// </summary>
    public string CampaignsFolderPath => Path.Combine(ClientProperties.DataFolderPath, "Addons", ShortName, "Campaigns");

    /// <summary>
    ///     Path to custom maps folder.
    /// </summary>
    public string MapsFolderPath => Path.Combine(ClientProperties.DataFolderPath, "Addons", ShortName, "Maps");

    /// <summary>
    ///     Path to autoload mods folder.
    /// </summary>
    public string ModsFolderPath => Path.Combine(ClientProperties.DataFolderPath, "Addons", ShortName, "Mods");

    /// <summary>
    ///     Does this game have skill levels.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Skills))]
    public bool AreSkillsAvailble => Skills is not null;


    /// <summary>
    ///     Game enum.
    /// </summary>
    public abstract GameEnum GameEnum { get; }

    /// <summary>
    ///     Full name of the game.
    /// </summary>
    public abstract string FullName { get; }

    /// <summary>
    ///     Short name of the game.
    /// </summary>
    public abstract string ShortName { get; }

    /// <summary>
    ///     List of files required for the base game to work.
    /// </summary>
    public abstract List<string> RequiredFiles { get; }

    /// <summary>
    ///     Enumeration of the available skill levels.
    ///     <see langword="null" /> if game doesn't have skills.
    /// </summary>
    public abstract Enum? Skills { get; }


    /// <summary>
    ///     Do provided files exist in the folder.
    /// </summary>
    /// <param name="files">List of required files.</param>
    /// <param name="path">Folder where the files are searched.</param>
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

    /// <summary>
    ///     Generates a list of zero-padded numbered filenames.
    /// </summary>
    /// <param name="baseName">Base name prefix (e.g. "TILES").</param>
    /// <param name="extension">File extension without dot (e.g. "ART").</param>
    /// <param name="start">Inclusive start index.</param>
    /// <param name="endExclusive">Exclusive end index.</param>
    protected static List<string> GenerateNumberedFiles(string baseName, string extension, int start, int endExclusive)
    {
        List<string> result = new(endExclusive - start);

        for (var i = start; i < endExclusive; i++)
        {
            result.Add($"{baseName}{i:000}.{extension}");
        }

        return result;
    }
}
