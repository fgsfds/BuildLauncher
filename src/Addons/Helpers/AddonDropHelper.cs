using Addons.Providers;
using Core.All.Enums;
using Core.Client.Helpers;
using Games.Games;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;

namespace Addons.Helpers;

public interface IAddonDropHelper
{
    Task<List<string>?> AddAddonsAsync(List<string> filePaths, BaseGame game);
}


public sealed class AddonDropHelper : IAddonDropHelper
{
    private readonly InstalledAddonsProviderFactory _installedAddonsProvider;
    private readonly ILogger<AddonDropHelper> _logger;

    public AddonDropHelper(
        InstalledAddonsProviderFactory installedAddonsProvider,
        ILogger<AddonDropHelper> logger
        )
    {
        _installedAddonsProvider = installedAddonsProvider;
        _logger = logger;
    }

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

        Ensure.DirectoryExists(folderToPutFile);

        var newPathToFile = Path.Combine(folderToPutFile, Path.GetFileName(pathToFile));

        File.Copy(pathToFile, newPathToFile, true);

        var provider = _installedAddonsProvider.Get(game);
        await provider.AddAddonAsync(newPathToFile).ConfigureAwait(false);

        return true;
    }

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
