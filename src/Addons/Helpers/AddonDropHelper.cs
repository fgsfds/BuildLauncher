using Addons.Providers;
using Core.All.Enums;
using Core.Client.Helpers;
using Core.Client.Interfaces;
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
    private readonly IUserNotifier _notifier;
    private readonly ILogger<AddonDropHelper> _logger;

    public AddonDropHelper(
        InstalledAddonsProviderFactory installedAddonsProvider,
        IUserNotifier notifier,
        ILogger<AddonDropHelper> logger
        )
    {
        _installedAddonsProvider = installedAddonsProvider;
        _notifier = notifier;
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
            var addedAddon = await AddAddonAsync(file, game).ConfigureAwait(false);

            if (addedAddon is not null)
            {
                _notifier.Show($"Added {addedAddon.Value.Type} for {addedAddon.Value.Game}.", NotificationSeverity.Success);

                continue;
            }

            failedInstalls ??= [];
            failedInstalls.Add(Path.GetFileName(file));
        }

        return failedInstalls;
    }

    private async Task<(GameEnum Game, AddonTypeEnum Type)?> AddAddonAsync(string pathToFile, BaseGame game)
    {
        if (!pathToFile.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
            !pathToFile.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError("File is not .zip or .map.");
            _notifier.Show($"{pathToFile} is not .zip or .map.", NotificationSeverity.Error);

            return null;
        }

        var addon = await GetGameAndTypeFromFileAsync(pathToFile, game.GameEnum).ConfigureAwait(false);

        if (addon is null)
        {
            _logger.LogError("Can't get addon from the file.");
            _notifier.Show($"Can't get addon from {pathToFile}.", NotificationSeverity.Error);

            return null;
        }

        if (addon.Value.Game != game.GameEnum)
        {
            _logger.LogError("Addon is for the wrong game: {Game}.", addon.Value.Game);
            _notifier.Show($"Addon {pathToFile} is for the wrong game: {addon.Value.Game}.", NotificationSeverity.Error);

            return null;
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
            _logger.LogError("Unknown addon type: {AddonType}.", addon.Value.AddonType);
            _notifier.Show($"Unknown addon type: {addon.Value.AddonType}.", NotificationSeverity.Error);

            return null;
        }

        Ensure.DirectoryExists(folderToPutFile);

        var newPathToFile = Path.Combine(folderToPutFile, Path.GetFileName(pathToFile));

        File.Copy(pathToFile, newPathToFile, true);

        var provider = _installedAddonsProvider.Get(game);
        await provider.AddAddonAsync(newPathToFile).ConfigureAwait(false);

        return addon;
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
