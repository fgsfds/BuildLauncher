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
    ///     Shared empty addon folders collection returned by games without addons.
    /// </summary>
    private static readonly IReadOnlyDictionary<Enum, string> EmptyAddonsFolders = new Dictionary<Enum, string>();

    /// <summary> Synchronizes access to the cached install-folder data. </summary>
    private readonly Lock _cacheLock = new();

    /// <summary> The install folder the cached data was built for. </summary>
    private string? _cacheFolder;

    /// <summary> Cached addon folders. Rebuilt when <see cref="GameInstallFolder" /> changes or the cache is invalidated. </summary>
    private IReadOnlyDictionary<Enum, string>? _addonsFoldersCache;

    /// <summary> Cached base game install check. Rebuilt when <see cref="GameInstallFolder" /> changes or the cache is invalidated. </summary>
    private bool? _isBaseGameInstalledCache;

    /// <summary>
    ///     Game install folder.
    /// </summary>
    public string? GameInstallFolder { get; set; }

    /// <summary>
    ///     Is base game installed.
    /// </summary>
    public bool IsBaseGameInstalled
    {
        get
        {
            var folder = GameInstallFolder;

            if (folder is null)
            {
                return IsInstalled(RequiredFiles);
            }

            lock (_cacheLock)
            {
                if (!string.Equals(_cacheFolder, folder, StringComparison.OrdinalIgnoreCase))
                {
                    ResetCaches(folder);
                }

                return _isBaseGameInstalledCache ??= IsInstalled(RequiredFiles);
            }
        }
    }

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
    public bool AreSkillsAvailable => Skills is not null;

    /// <summary> Collection of additional folders that contain game data or its addons. </summary>
    public IReadOnlyList<string> AdditionalFolders => [.. AddonsFolders.Values.Distinct(StringComparer.OrdinalIgnoreCase)];

    /// <summary> Collection of paths to folders that contain game's addons. </summary>
    public IReadOnlyDictionary<Enum, string> AddonsFolders
    {
        get
        {
            var folder = GameInstallFolder;

            if (folder is null)
            {
                return DetectAddonsFolders();
            }

            lock (_cacheLock)
            {
                if (!string.Equals(_cacheFolder, folder, StringComparison.OrdinalIgnoreCase))
                {
                    ResetCaches(folder);
                }

                return _addonsFoldersCache ??= DetectAddonsFolders();
            }
        }
    }

    /// <summary> Invalidates any cached addon detection and base-game install data so it is recomputed on the next access. </summary>
    public void InvalidateAddonsCache()
    {
        lock (_cacheLock)
        {
            _addonsFoldersCache = null;
            _isBaseGameInstalledCache = null;
            _cacheFolder = null;
        }
    }

    /// <summary> Resets the cached data and records the install folder the next context is built for. </summary>
    /// <param name="folder"> The install folder of the new context. </param>
    private void ResetCaches(string folder)
    {
        _addonsFoldersCache = null;
        _isBaseGameInstalledCache = null;
        _cacheFolder = folder;
    }

    /// <summary> Addons that are detected in the game install folder. </summary>
    protected virtual IReadOnlyCollection<Enum> SupportedAddons => [];

    /// <summary> Detects the folders that contain the game's addons. </summary>
    /// <returns> A dictionary mapping detected addons to their folders. </returns>
    protected virtual IReadOnlyDictionary<Enum, string> DetectAddonsFolders() => EmptyAddonsFolders;


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
    protected abstract IReadOnlyCollection<string> RequiredFiles { get; }

    /// <summary>
    ///     Enumeration of the available skill levels.
    ///     <see langword="null" /> if game doesn't have skills.
    /// </summary>
    public abstract Enum? Skills { get; }


    /// <summary>
    ///     Do provided files exist in the folder.
    /// </summary>
    /// <param name="files">
    ///     List of required files.
    /// </param>
    /// <param name="path">
    ///     Folder where the files are searched.
    /// </param>
    protected bool IsInstalled(IReadOnlyCollection<string> files, string? path = null)
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
    /// <param name="baseName">
    ///     Base name prefix (e.g. "TILES").
    /// </param>
    /// <param name="extension">
    ///     File extension without dot (e.g. "ART").
    /// </param>
    /// <param name="start">
    ///     Inclusive start index.
    /// </param>
    /// <param name="endExclusive">
    ///     Exclusive end index.
    /// </param>
    /// <param name="padWidth">
    ///     Zero-padding width.
    /// </param>
    protected static IReadOnlyCollection<string> GenerateNumberedFiles(string baseName, string extension, int start, int endExclusive, int padWidth)
    {
        List<string> result = new(endExclusive - start);
        var format = $"{baseName}{{0:D{padWidth}}}.{extension}";

        for (var i = start; i < endExclusive; i++)
        {
            result.Add(string.Format(format, i));
        }

        return result;
    }
}
