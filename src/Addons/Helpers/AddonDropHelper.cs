using System.Text.Json;
using Addons.Providers;
using Core.All.Enums;
using Core.All.Serializable.Addon;
using Games.Games;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;

public interface IAddonDropHelper
{
    /// <summary>
    /// Attempts to add multiple addons to the specified game.
    /// </summary>
    /// <param name="filePaths">A list of file paths representing the addons to be added.</param>
    /// <param name="game">The target game to which the addons should be added.</param>
    /// <returns>A list of file paths for addons that failed to install or null if all installations were successful.</returns>
    Task<List<string>?> AddAddonsAsync(List<string> filePaths, BaseGame game);
}

public sealed class AddonDropHelper : IAddonDropHelper
{
    private readonly InstalledAddonsProviderFactory _installedAddonsProvider;
    private readonly ILogger<AddonDropHelper> _logger;

    public AddonDropHelper(InstalledAddonsProviderFactory installedAddonsProvider, ILogger<AddonDropHelper> logger)
    {
        _installedAddonsProvider = installedAddonsProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<string>?> AddAddonsAsync(List<string> filePaths, BaseGame game)
    {
        if (filePaths.Count == 0)
        {
            return null;
        }

        List<string> failedInstalls = [];

        foreach (var file in filePaths)
        {
            var isAdded = await AddAddonAsync(file, game).ConfigureAwait(false);

            if (!isAdded)
            {
                failedInstalls.Add(Path.GetFileName(file));
            }
        }

        if (failedInstalls.Count == 0)
        {
            return null;
        }

        return failedInstalls;
    }


    /// <summary>
    /// Attempts to add an addon to the specified game.
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

        var addon = GetGameAndTypeFromFile(pathToFile, game);

        if (addon is null)
        {
            _logger.LogError("Can't get addon from the file.");
            return false;
        }

        if (addon.Item1 != game.GameEnum)
        {
            _logger.LogError($"Addon is for the wrong game: {addon.Item1}.");
            return false;
        }

        string folderToPutFile;

        if (addon.Item2 is AddonTypeEnum.TC)
        {
            folderToPutFile = game.CampaignsFolderPath;
        }
        else if (addon.Item2 is AddonTypeEnum.Map)
        {
            folderToPutFile = game.MapsFolderPath;
        }
        else if (addon.Item2 is AddonTypeEnum.Mod)
        {
            folderToPutFile = game.ModsFolderPath;
        }
        else
        {
            _logger.LogError($"Unknown addon type: {addon.Item2}.");
            return false;
        }

        var newPathToFile = Path.Combine(folderToPutFile, Path.GetFileName(pathToFile));

        File.Copy(pathToFile, newPathToFile, true);

        using var installer = _installedAddonsProvider.Get(game);
        await installer.AddAddonAsync(newPathToFile).ConfigureAwait(false);

        return true;
    }

    /// <summary>
    /// Get game enum and addon type enum from a file.
    /// </summary>
    /// <param name="pathToFile">Path to file.</param>
    private Tuple<GameEnum, AddonTypeEnum>? GetGameAndTypeFromFile(string pathToFile, BaseGame game)
    {
        if (ArchiveFactory.IsArchive(pathToFile, out _))
        {
            using var archive = ArchiveFactory.OpenArchive(pathToFile);
            var manifestFile = archive.Entries.FirstOrDefault(static x => x.Key!.Equals("addon.json", StringComparison.OrdinalIgnoreCase));

            if (manifestFile is null)
            {
                return null;
            }

            using var stream = manifestFile.OpenEntryStream();

            var manifest = JsonSerializer.Deserialize(
                stream,
                AddonManifestContext.Default.AddonJsonModel
                );

            if (manifest is null)
            {
                return null;
            }

            var supportedGame = manifest.SupportedGame.Game;
            var type = manifest.AddonType;

            return new(supportedGame, type);
        }

        if (pathToFile.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
        {
            return new(game.GameEnum, AddonTypeEnum.Map);
        }

        return null;
    }
}
