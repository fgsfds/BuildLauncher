using System.Text.Json;
using Core.All;
using Core.All.Enums;
using Core.All.Serializable;
using Core.All.Serializable.Addon;
using Core.All.Serializable.Downloadable;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Microsoft.Extensions.Logging;

namespace Core.Client.Api;

public sealed class OfflineApiInterface : IApiInterface
{
    private readonly ILogger<OfflineApiInterface> _logger;

    private readonly SemaphoreSlim _semaphore = new(1);

    private Dictionary<GameEnum, List<DownloadableAddonJsonModel>>? _addonsJson;

    public OfflineApiInterface(ILogger<OfflineApiInterface> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<DownloadableAddonJsonModel>?> GetAddonsAsync(GameEnum gameEnum, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_addonsJson is null)
            {
                if (ClientProperties.PathToLocalAddonsJson is null)
                {
                    return null;
                }

                using var addonsJson = File.OpenRead(ClientProperties.PathToLocalAddonsJson);

                _addonsJson = await JsonSerializer.DeserializeAsync(
                    addonsJson,
                    DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel,
                    cancellationToken
                    ).ConfigureAwait(false);

                ArgumentNullException.ThrowIfNull(_addonsJson);
            }

            if (gameEnum is GameEnum.Redneck)
            {
                _ = _addonsJson.TryGetValue(GameEnum.Redneck, out var rrAddons);
                _ = _addonsJson.TryGetValue(GameEnum.RidesAgain, out var againAddons);

                return
                [
                    .. rrAddons ?? [],
                    .. againAddons ?? []
                ];
            }

            if (gameEnum is GameEnum.Witchaven)
            {
                _ = _addonsJson.TryGetValue(GameEnum.Witchaven, out var w1Addons);
                _ = _addonsJson.TryGetValue(GameEnum.Witchaven2, out var w2Addons);

                return
                [
                    .. w1Addons ?? [],
                    .. w2Addons ?? []
                ];
            }

            return _addonsJson.TryGetValue(gameEnum, out var result) ? result : [];
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while getting addonsJson from GitHub ===");

            return null;
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public Task<GeneralReleaseJsonModel?> GetLatestAppReleaseAsync(CancellationToken cancellationToken = default) => Task.FromResult<GeneralReleaseJsonModel?>(null);

    /// <inheritdoc />
    public Task<GeneralReleaseJsonModel?> GetLatestPortReleaseAsync(PortEnum portEnum, CancellationToken cancellationToken = default) => Task.FromResult<GeneralReleaseJsonModel?>(null);

    /// <inheritdoc />
    public Task<GeneralReleaseJsonModel?> GetLatestToolReleaseAsync(ToolEnum toolEnum, CancellationToken cancellationToken = default) => Task.FromResult<GeneralReleaseJsonModel?>(null);

    /// <inheritdoc />
    public Task<bool> AddAddonToDatabaseAsync(AddonManifestJsonModel addonJson, DownloadableAddonJsonModel downloadableAddonJson) => Task.FromResult(false);

    /// <inheritdoc />
    public async Task<string?> GetUploadFolderAsync(CancellationToken cancellationToken = default)
    {
        using var dataJson = File.OpenRead(ClientProperties.PathToLocalDataJson);

        var data = await JsonSerializer.DeserializeAsync(
            dataJson,
            DataJsonModelContext.Default.DictionaryStringString,
            cancellationToken
            ).ConfigureAwait(false);

        if (data is null)
        {
            return null;
        }

        _ = data.TryGetValue(DataJson.UploadFolder, out var uploadFolder) ? uploadFolder : null;

        return uploadFolder;
    }

    /// <inheritdoc />
    public async Task<List<AddonManifestJsonModel>?> GetMetadataAsync(CancellationToken cancellationToken = default)
    {
        using var dataJson = File.OpenRead(ClientProperties.PathToLocalManifestsJson);

        var data = await JsonSerializer.DeserializeAsync(
            dataJson,
            AddonManifestJsonContext.Default.ListAddonManifestJsonModel,
            cancellationToken
            ).ConfigureAwait(false);

        return data;
    }

    /// <inheritdoc />
    public async Task<Result<Uri?>> GetSignedUrlAsync(string path, CancellationToken cancellationToken = default)
    {
        var uploadFolder = await GetUploadFolderAsync(cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(uploadFolder))
        {
            return new(ResultEnum.Error, null, "Error while getting signed url.");
        }

        var url = Path.Combine(uploadFolder, path);

        return new(ResultEnum.Success, new(url), string.Empty);
    }


    #region Not Implemented

    /// <inheritdoc />
    public Task<decimal?> ChangeScoreAsync(string addonId, sbyte score, bool isNew) => Task.FromResult<decimal?>(null);

    /// <inheritdoc />
    public Task<Dictionary<string, decimal>?> GetRatingsAsync() => Task.FromResult<Dictionary<string, decimal>?>(null);

    /// <inheritdoc />
    public Task<bool> IncreaseNumberOfInstallsAsync(string addonId) => Task.FromResult(false);

    #endregion
}
