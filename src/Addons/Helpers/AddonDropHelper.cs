using Addons.Providers;
using Core.All.Enums;
using Core.Client.Helpers;
using Games.Games;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;

namespace Addons.Helpers;

/// <summary>
///     Provides functionality for adding addons to a game.
/// </summary>
public interface IAddonDropHelper
{
    /// <summary>
    ///     Attempts to add multiple addons to the specified game.
    /// </summary>
    /// <param name="filePaths">A list of file paths representing the addons to be added.</param>
    /// <param name="game">The target game to which the addons should be added.</param>
    /// <returns>A list of file paths for addons that failed to install or null if all installations were successful.</returns>
    Task<List<string>?> AddAddonsAsync(List<string> filePaths, BaseGame game);
}


/// <summary>
///     Handles dropping addon files into the appropriate game folder.
/// </summary>
public sealed class AddonDropHelper : IAddonDropHelper
{
    private readonly LocalFilesProvider _addonScanner;
    private readonly ILogger<AddonDropHelper> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonDropHelper" /> class.
    /// </summary>
    /// <param name="addonScanner">Provider used to scan and cache parsed addon files.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    public AddonDropHelper(
        LocalFilesProvider addonScanner,
        ILogger<AddonDropHelper> logger
        )
    {
        _addonScanner = addonScanner;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<string>?> AddAddonsAsync(List<string> filePaths, BaseGame game)
    {
        if (filePaths.Count == 0)
        {
            return null;
        }

        List<string>? failedInstalls = null;

        foreach (var file in filePaths)
        {
            var isAdded = await AddAddonAsync(file, game).ConfigureAwait(false);

            if (isAdded)
            {
                continue;
            }

            failedInstalls ??= [];
            failedInstalls.Add(Path.GetFileName(file));
        }

        return failedInstalls;
    }


    /// <summary>
    ///     Attempts to add an addon to the specified game.
    /// </summary>
    /// <param name="pathToFile">The path to the addon file.</param>
    /// <param name="game">The target game to which the addon should be added.</param>
    /// <returns>True if the addon was added successfully, false otherwise.</returns>
    private async Task<bool> AddAddonAsync(string pathToFile, BaseGame game)
    {
        if (!pathToFile.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
            !pathToFile.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError("File is not .zip or .map.");

            return false;
        }

        var addon = await GetGameAndTypeFromFileAsync(pathToFile, game.GameEnum).ConfigureAwait(false);

        if (addon is null)
        {
            _logger.LogError("Can't get addon from the file.");

            return false;
        }

        if (addon.Value.Game != game.GameEnum)
        {
            _logger.LogError($"Addon is for the wrong game: {addon.Value.Game}.");

            return false;
        }

        string folderToPutFile;

        if (addon.Value.AddonType is AddonTypeEnum.TC)
        {
            folderToPutFile = game.CampaignsFolderPath;
        }
        else if (addon.Value.AddonType is AddonTypeEnum.Map)
        {
            folderToPutFile = game.MapsFolderPath;
        }
        else if (addon.Value.AddonType is AddonTypeEnum.Mod)
        {
            folderToPutFile = game.ModsFolderPath;
        }
        else
        {
            _logger.LogError($"Unknown addon type: {addon.Value.AddonType}.");

            return false;
        }

        var newPathToFile = Path.Combine(folderToPutFile, Path.GetFileName(pathToFile));

        File.Copy(pathToFile, newPathToFile, true);

        var parsedFiles = await _addonScanner.TryAddFileToCacheAsync(newPathToFile, game.GameEnum).ConfigureAwait(false);

        if (parsedFiles is null)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Get game enum and addon type enum from a file.
    /// </summary>
    /// <param name="pathToFile">Path to file.</param>
    /// <param name="gameEnum">The game to associate with standalone .map files.</param>
    private async Task<(GameEnum Game, AddonTypeEnum AddonType)?> GetGameAndTypeFromFileAsync(string pathToFile, GameEnum gameEnum)
    {
        if (ArchiveFactory.IsArchive(pathToFile, out _))
        {
            var manifest = await ManifestHelper.GetMainManifestAsync(pathToFile).ConfigureAwait(false);

            if (!manifest.IsSuccess)
            {
                return null;
            }

            var supportedGame = manifest.ResultObject.SupportedGame.Game;
            var type = manifest.ResultObject.AddonType;

            return new(supportedGame, type);
        }

        if (pathToFile.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
        {
            return new(gameEnum, AddonTypeEnum.Map);
        }

        return null;
    }
}
