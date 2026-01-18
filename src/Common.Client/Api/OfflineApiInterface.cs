using System.Text.Json;
using Common.All.Enums;
using Common.All.Serializable.Downloadable;
using Common.Client.Helpers;
using Common.Client.Interfaces;
using Microsoft.Extensions.Logging;

namespace Common.Client.Api;

public sealed class OfflineApiInterface : IApiInterface
{
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _semaphore = new(1);

    private Dictionary<GameEnum, List<DownloadableAddonJsonModel>>? _addonsJson;

    public OfflineApiInterface(
        ILogger logger
        )
    {
        _logger = logger;
    }

    public async Task<List<DownloadableAddonJsonModel>?> GetAddonsAsync(GameEnum gameEnum)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            if (_addonsJson is null)
            {
                if (ClientProperties.PathToLocalAddonsJson is null)
                {
                    return null;
                }

                var addons = File.ReadAllText(ClientProperties.PathToLocalAddonsJson);

                _addonsJson = JsonSerializer.Deserialize(addons, DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel);

                if (_addonsJson is null)
                {
                    throw new ArgumentNullException();
                }
            }

            if (gameEnum is GameEnum.Redneck)
            {
                _ = _addonsJson.TryGetValue(GameEnum.Redneck, out var rrAddons);
                _ = _addonsJson.TryGetValue(GameEnum.RidesAgain, out var againAddons);

                return [.. rrAddons ?? [], .. againAddons ?? []];
            }

            if (gameEnum is GameEnum.Witchaven)
            {
                _ = _addonsJson.TryGetValue(GameEnum.Witchaven, out var w1Addons);
                _ = _addonsJson.TryGetValue(GameEnum.Witchaven2, out var w2Addons);

                return [.. w1Addons ?? [], .. w2Addons ?? []];
            }

            return _addonsJson.TryGetValue(gameEnum, out var result) ? result : [];
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while getting addons from GitHub ===");
            return null;
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }

    public Task<GeneralReleaseJsonModel?> GetLatestAppReleaseAsync() => Task.FromResult<GeneralReleaseJsonModel?>(null);

    public Task<GeneralReleaseJsonModel?> GetLatestPortReleaseAsync(PortEnum portEnum) => Task.FromResult<GeneralReleaseJsonModel?>(null);

    public Task<GeneralReleaseJsonModel?> GetLatestToolReleaseAsync(ToolEnum toolEnum) => Task.FromResult<GeneralReleaseJsonModel?>(null);

    public Task<bool> AddAddonToDatabaseAsync(DownloadableAddonJsonModel addon)
    {
        if (ClientProperties.PathToLocalAddonsJson is null)
        {
            throw new FormatException("Can't find local addons.json");
            return Task.FromResult(false);
        }

        var addonsJson = File.ReadAllText(ClientProperties.PathToLocalAddonsJson);
        var addons = JsonSerializer.Deserialize(addonsJson, DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel);

        if (addons is null)
        {
            throw new FormatException("Error while deserializing addons.json");
            return Task.FromResult(false);
        }

        if (!addons.TryGetValue(addon.Game, out _))
        {
            addons[addon.Game] = [];
        }

        var existingAddon = addons[addon.Game].FirstOrDefault(x => x.Id.Equals(addon.Id));

        if (existingAddon is not null)
        {
            _ = addons[addon.Game].Remove(existingAddon);
        }

        for (var i = 0; i < addon.Dependencies?.Count; i++)
        {
            var readableName = addons[addon.Game].FirstOrDefault(x => x.Id.Equals(addon.Dependencies[i]));

            if (readableName is not null)
            {
                addon.Dependencies[i] = readableName.Title;
            }
        }

        addons[addon.Game].Add(addon);

        var newAddonsJson = JsonSerializer.Serialize(addons, DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel);
        File.WriteAllText(ClientProperties.PathToLocalAddonsJson, newAddonsJson);

        return Task.FromResult(true);
    }

    public Task<string?> GetUploadFolder() => Task.FromResult<string?>(null);


    #region Not Implemented

    public Task<string?> GetSignedUrlAsync(string path) => Task.FromResult<string?>(null);

    public Task<decimal?> ChangeScoreAsync(string addonId, sbyte score, bool isNew) => Task.FromResult<decimal?>(null);

    public Task<Dictionary<string, decimal>?> GetRatingsAsync() => Task.FromResult<Dictionary<string, decimal>?>(null);

    public Task<bool> IncreaseNumberOfInstallsAsync(string addonId) => Task.FromResult(false);

    #endregion
}
