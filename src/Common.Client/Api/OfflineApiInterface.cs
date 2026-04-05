using System.Text.Json;
using Common.All;
using Common.All.Enums;
using Common.All.Serializable;
using Common.All.Serializable.Addon;
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

    public OfflineApiInterface(ILogger logger)
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

    public Task<bool> AddAddonToDatabaseAsync(AddonJsonModel addonJson, DownloadableAddonJsonModel downloadableAddonJson) => Task.FromResult(false);

    public async Task<string?> GetUploadFolderAsync()
    {
        var dataJson = await File.ReadAllTextAsync(ClientProperties.PathToLocalDataJson).ConfigureAwait(false);
        var data = JsonSerializer.Deserialize(dataJson, DataJsonModelContext.Default.DictionaryStringString);

        _ = data!.TryGetValue(DataJson.UploadFolder, out var uploadFolder) ? uploadFolder : null;

        return uploadFolder;
    }

    public async Task<List<AddonJsonModel>?> GetMetadataAsync()
    {
        var dataJson = await File.ReadAllTextAsync(ClientProperties.PathToLocalManifestsJson).ConfigureAwait(false);
        var data = JsonSerializer.Deserialize(dataJson, ManifestsJsonModelContext.Default.ListAddonJsonModel);

        return data;
    }

    public async Task<Result<string?>> GetSignedUrlAsync(string path)
    {
        var uploadFolder = await GetUploadFolderAsync().ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(uploadFolder))
        {
            return new(ResultEnum.Error, null, "Error while getting signed url.");
        }

        var url = Path.Combine(uploadFolder, path);

        return new(ResultEnum.Success, url, string.Empty);
    }


    #region Not Implemented

    public Task<decimal?> ChangeScoreAsync(string addonId, sbyte score, bool isNew) => Task.FromResult<decimal?>(null);

    public Task<Dictionary<string, decimal>?> GetRatingsAsync() => Task.FromResult<Dictionary<string, decimal>?>(null);

    public Task<bool> IncreaseNumberOfInstallsAsync(string addonId) => Task.FromResult(false);

    #endregion
}
