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

/// <summary>
///     API interface implementation that serves data from local files for offline use.
/// </summary>
public sealed class OfflineApiInterface : IApiInterface
{
    private readonly ILogger<OfflineApiInterface> _logger;

    private readonly SemaphoreSlim _semaphore = new(1);

    private Dictionary<GameEnum, List<DownloadableAddonJsonModel>>? _addonsJson;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OfflineApiInterface" /> class.
    /// </summary>
    /// <param name="logger">
    ///     The logger.
    /// </param>
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
                    _logger.LogWarning("Local addons.json path is not set; cannot load addons");

                    return null;
                }

                using var addonsJson = File.OpenRead(ClientProperties.PathToLocalAddonsJson);

                _addonsJson = await JsonSerializer.DeserializeAsync(
                    addonsJson,
                    DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel!,
                    cancellationToken
                    ).ConfigureAwait(false);

                ArgumentNullException.ThrowIfNull(_addonsJson);

                _logger.LogInformation("Loaded local addons.json from {Path} with {Count} games", ClientProperties.PathToLocalAddonsJson, _addonsJson.Count);
            }

            if (gameEnum is GameEnum.Redneck)
            {
                _ = _addonsJson.TryGetValue(GameEnum.Redneck, out var rrAddons);
                _ = _addonsJson.TryGetValue(GameEnum.RidesAgain, out var againAddons);

                List<DownloadableAddonJsonModel> redneckAddons = [.. rrAddons ?? [], .. againAddons ?? []];

                _logger.LogInformation("Returning {Count} addons for {Game}", redneckAddons.Count, gameEnum);

                return redneckAddons;
            }

            if (gameEnum is GameEnum.Witchaven)
            {
                _ = _addonsJson.TryGetValue(GameEnum.Witchaven, out var w1Addons);
                _ = _addonsJson.TryGetValue(GameEnum.Witchaven2, out var w2Addons);

                List<DownloadableAddonJsonModel> witchavenAddons = [.. w1Addons ?? [], .. w2Addons ?? []];

                _logger.LogInformation("Returning {Count} addons for {Game}", witchavenAddons.Count, gameEnum);

                return witchavenAddons;
            }

            var gameAddons = _addonsJson.TryGetValue(gameEnum, out var result) ? result : [];

            _logger.LogInformation("Returning {Count} addons for {Game}", gameAddons.Count, gameEnum);

            return gameAddons;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while getting local addons.json ===");

            return null;
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public Task<GeneralReleaseJsonModel?> GetLatestAppReleaseAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Offline API does not provide app releases; returning no release");

        return Task.FromResult<GeneralReleaseJsonModel?>(null);
    }

    /// <inheritdoc />
    public Task<GeneralReleaseJsonModel?> GetLatestPortReleaseAsync(PortEnum portEnum, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Offline API does not provide releases for port {Port}; returning no release", portEnum);

        return Task.FromResult<GeneralReleaseJsonModel?>(null);
    }

    /// <inheritdoc />
    public Task<GeneralReleaseJsonModel?> GetLatestToolReleaseAsync(ToolEnum toolEnum, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Offline API does not provide releases for tool {Tool}; returning no release", toolEnum);

        return Task.FromResult<GeneralReleaseJsonModel?>(null);
    }

    /// <inheritdoc />
    public Task<bool> AddAddonToDatabaseAsync(AddonManifestJsonModel addonJson, DownloadableAddonJsonModel downloadableAddonJson)
    {
        _logger.LogWarning("Offline API does not support adding addons to the database; ignoring addon {AddonId}", addonJson?.Id);

        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, string>?> GetDataJsonAsync(CancellationToken cancellationToken = default)
    {
        if (ClientProperties.PathToLocalDataJson is null)
        {
            _logger.LogWarning("Local data.json path is not set; cannot load data");

            return null;
        }

        try
        {
            var dataJson = File.OpenRead(ClientProperties.PathToLocalDataJson);
            await using var dataJsonScope = dataJson.ConfigureAwait(false);

            var data = await JsonSerializer.DeserializeAsync(
                dataJson,
                DataJsonModelContext.Default.DictionaryStringString!,
                cancellationToken
                ).ConfigureAwait(false);

            _logger.LogInformation("Loaded local data.json from {Path} with {Count} entries", ClientProperties.PathToLocalDataJson, data?.Count ?? 0);

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while getting local data.json ===");

            return null;
        }
    }

    /// <inheritdoc />
    public async Task<string?> GetUploadFolderAsync(CancellationToken cancellationToken = default)
    {
        var data = await GetDataJsonAsync(cancellationToken).ConfigureAwait(false);

        if (data?.TryGetValue(DataJson.UploadFolder, out var uploadFolder) == true)
        {
            _logger.LogInformation("Upload folder resolved from local data.json: {UploadFolder}", uploadFolder);

            return uploadFolder;
        }

        _logger.LogWarning("Upload folder is missing in local data.json");

        return null;
    }

    /// <inheritdoc />
    public async Task<List<AddonManifestJsonModel>?> GetMetadataAsync(CancellationToken cancellationToken = default)
    {
        if (ClientProperties.PathToLocalManifestsJson is null)
        {
            _logger.LogWarning("Local manifests.json path is not set; cannot load metadata");

            return null;
        }

        try
        {
            using var dataJson = File.OpenRead(ClientProperties.PathToLocalManifestsJson);

            var data = await JsonSerializer.DeserializeAsync(
                dataJson,
                AddonManifestJsonContext.Default.ListAddonManifestJsonModel!,
                cancellationToken
                ).ConfigureAwait(false);

            _logger.LogInformation("Loaded {Count} addon manifests from local {Path}", data?.Count ?? 0, ClientProperties.PathToLocalManifestsJson);

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while getting local manifests.json ===");

            return null;
        }
    }

    /// <inheritdoc />
    public async Task<Result<Uri?>> GetSignedUrlAsync(string path, CancellationToken cancellationToken = default)
    {
        var uploadFolder = await GetUploadFolderAsync(cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(uploadFolder))
        {
            _logger.LogWarning("Failed to build signed URL for {Path}: upload folder not found", path);

            return new(ResultEnum.Error, null, "Error while getting signed url.");
        }

        var url = Path.Combine(uploadFolder, path);

        _logger.LogInformation("Built signed URL for {Path}", path);

        return new(ResultEnum.Success, new(url), string.Empty);
    }


    #region Not Implemented

    /// <inheritdoc />
    public Task<decimal?> ChangeScoreAsync(string addonId, sbyte score, bool isNew)
    {
        _logger.LogDebug("Offline API does not support score changes; ignoring score change for addon {AddonId}", addonId);

        return Task.FromResult<decimal?>(null);
    }

    /// <inheritdoc />
    public Task<Dictionary<string, decimal>?> GetRatingsAsync()
    {
        _logger.LogDebug("Offline API does not provide ratings; returning no ratings");

        return Task.FromResult<Dictionary<string, decimal>?>(null);
    }

    /// <inheritdoc />
    public Task<bool> IncreaseNumberOfInstallsAsync(string addonId)
    {
        _logger.LogDebug("Offline API does not track install counts; ignoring install count for addon {AddonId}", addonId);

        return Task.FromResult(false);
    }

    #endregion
}
