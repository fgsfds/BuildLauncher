using System.Text.Json;
using Core.All;
using Core.All.Enums;
using Core.All.Helpers;
using Core.All.Releases;
using Core.All.Serializable;
using Core.All.Serializable.Addon;
using Core.All.Serializable.Downloadable;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Microsoft.Extensions.Logging;

namespace Core.Client.Api;

/// <summary>
///     Provides GitHub-backed implementation of the API interface for releases, addons, and metadata.
/// </summary>
public sealed class GitHubApiInterface : IApiInterface
{
    private readonly ReleaseProviderBase<AppReleaseEnum> _appRepoReleasesProvider;

    private readonly IHttpClientFactory _httpClientFactory;

    private readonly ILogger<GitHubApiInterface> _logger;

    private readonly ReleaseProviderBase<PortEnum> _portsReleasesProviderBase;

    /// <summary>
    ///     Semaphore for synchronizing access to cached data.
    /// </summary>
    private readonly SemaphoreSlim _semaphore = new(1);

    private readonly ReleaseProviderBase<ToolEnum> _toolsReleasesProviderBase;

    /// <summary>
    ///     Cached addon data loaded from the remote addons.json.
    /// </summary>
    private Dictionary<GameEnum, List<DownloadableAddonJsonModel>>? _addonsJson;

    /// <summary>
    ///     Cached data from the remote data.json.
    /// </summary>
    private Dictionary<string, string>? _data;


    /// <summary>
    ///     Initializes a new instance of <see cref="GitHubApiInterface" />.
    /// </summary>
    /// <param name="portsReleasesProviderBase">Provider for port releases.</param>
    /// <param name="toolsReleasesRetriever">Provider for tool releases.</param>
    /// <param name="appRepoReleasesProvider">Provider for app self-update releases.</param>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="logger">Logger instance.</param>
    public GitHubApiInterface(
        ReleaseProviderBase<PortEnum> portsReleasesProviderBase,
        ReleaseProviderBase<ToolEnum> toolsReleasesRetriever,
        ReleaseProviderBase<AppReleaseEnum> appRepoReleasesProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<GitHubApiInterface> logger
        )
    {
        _portsReleasesProviderBase = portsReleasesProviderBase;
        _toolsReleasesProviderBase = toolsReleasesRetriever;
        _appRepoReleasesProvider = appRepoReleasesProvider;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }


    /// <inheritdoc />
    public async Task<List<DownloadableAddonJsonModel>?> GetAddonsAsync(GameEnum gameEnum)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            if (_addonsJson is null)
            {
                using var httpClient = _httpClientFactory.CreateClient(HttpClientEnum.GitHub.GetDescription());
                using var response = await httpClient.GetStreamAsync(CommonConstants.AddonsJsonUrl).ConfigureAwait(false);

                _addonsJson = await JsonSerializer.DeserializeAsync(
                    response,
                    DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel
                    ).ConfigureAwait(false);

                if (_addonsJson is null)
                {
                    throw new FormatException("Error while deserializing addons.json");
                }
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
            _logger.LogCritical(ex, "=== Error while getting addons from GitHub ===");

            return null;
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<GeneralReleaseJsonModel?> GetLatestAppReleaseAsync()
    {
        var result = await _appRepoReleasesProvider.GetLatestReleaseAsync(AppReleaseEnum.MainApp, ClientProperties.IsDeveloperMode).ConfigureAwait(false);

        if (result?.TryGetValue(CommonProperties.OSEnum, out var release) is true)
        {
            return release;
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<GeneralReleaseJsonModel?> GetLatestPortReleaseAsync(PortEnum portEnum)
    {
        var result = await _portsReleasesProviderBase.GetLatestReleaseAsync(portEnum, false).ConfigureAwait(false);

        if (result?.TryGetValue(CommonProperties.OSEnum, out var release) is true)
        {
            return release;
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<GeneralReleaseJsonModel?> GetLatestToolReleaseAsync(ToolEnum toolEnum)
    {
        var result = await _toolsReleasesProviderBase.GetLatestReleaseAsync(toolEnum, false).ConfigureAwait(false);

        if (result?.TryGetValue(CommonProperties.OSEnum, out var release) is true)
        {
            return release;
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<bool> AddAddonToDatabaseAsync(AddonManifestJsonModel addonJson, DownloadableAddonJsonModel downloadableAddonJson)
    {
        if (ClientProperties.PathToLocalAddonsJson is null)
        {
            _logger.LogError("Can't find local addons.json");

            return false;
        }

        if (ClientProperties.PathToLocalManifestsJson is null)
        {
            _logger.LogError("Can't find local manifests.json");

            return false;
        }

        Dictionary<GameEnum, List<DownloadableAddonJsonModel>>? addons;

        using (var addonsJson = File.OpenRead(ClientProperties.PathToLocalAddonsJson))
        {
            addons = await JsonSerializer.DeserializeAsync(
                addonsJson,
                DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel
                ).ConfigureAwait(false);
        }

        if (addons is null)
        {
            _logger.LogError("Error while deserializing addons.json");

            return false;
        }

        if (!addons.TryGetValue(downloadableAddonJson.Game, out _))
        {
            addons[downloadableAddonJson.Game] = [];
        }

        var existingAddon = addons[downloadableAddonJson.Game].FirstOrDefault(x => x.Id.Equals(downloadableAddonJson.Id));

        if (existingAddon is not null)
        {
            _ = addons[downloadableAddonJson.Game].Remove(existingAddon);
        }

        for (var i = 0; i < downloadableAddonJson.Dependencies?.Count; i++)
        {
            var readableName = addons[downloadableAddonJson.Game].FirstOrDefault(x => x.Id.Equals(downloadableAddonJson.Dependencies[i]));

            if (readableName is not null)
            {
                downloadableAddonJson.Dependencies[i] = readableName.Title;
            }
        }

        addons[downloadableAddonJson.Game].Add(downloadableAddonJson);

        foreach (var add in addons)
        {
            List<DownloadableAddonJsonModel> sorted = [.. add.Value.OrderBy(x => x.Title)];
            add.Value.Clear();
            add.Value.AddRange(sorted);
        }

        var newAddonsJson = JsonSerializer.Serialize(addons, DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel);
        await File.WriteAllTextAsync(ClientProperties.PathToLocalAddonsJson, newAddonsJson).ConfigureAwait(false);

        List<AddonManifestJsonModel>? manifests;

        await using (var manifestsJson = File.OpenRead(ClientProperties.PathToLocalManifestsJson))
        {
            manifests = await JsonSerializer.DeserializeAsync(
                manifestsJson,
                AddonManifestJsonContext.Default.ListAddonManifestJsonModel
                ).ConfigureAwait(false);
        }

        if (manifests is null)
        {
            _logger.LogError("Error while deserializing manifests.json");

            return false;
        }

        manifests.RemoveAll(x => x.Id.Equals(addonJson.Id));
        manifests.Add(addonJson);

        manifests = [.. manifests.OrderBy(x => x.SupportedGame.Game).ThenBy(x => x.AddonType).ThenBy(x => x.Title)];

        var newManifestsJson = JsonSerializer.Serialize(manifests, AddonManifestJsonContext.Default.ListAddonManifestJsonModel);
        await File.WriteAllTextAsync(ClientProperties.PathToLocalManifestsJson, newManifestsJson).ConfigureAwait(false);

        return true;
    }

    /// <inheritdoc />
    public async Task<string?> GetUploadFolderAsync()
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            if (_data is null)
            {
                await InitDataAsync().ConfigureAwait(false);
            }

            return _data?.TryGetValue(DataJson.UploadFolder, out var uploadFolder) == true ? uploadFolder : null;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while getting upload folder from GitHub ===");

            return null;
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<List<AddonManifestJsonModel>?> GetMetadataAsync()
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();

            using var jsonStream = await httpClient.GetStreamAsync(CommonConstants.ManifestsJsonUrl).ConfigureAwait(false)
                                ?? throw new FormatException("Error while deserializing manifests.json");

            var meta = await JsonSerializer.DeserializeAsync(
                jsonStream,
                AddonManifestJsonContext.Default.ListAddonManifestJsonModel
                ).ConfigureAwait(false);

            return meta;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while getting upload folder from GitHub ===");

            return null;
        }
    }

    /// <inheritdoc />
    public async Task<Result<Uri?>> GetSignedUrlAsync(string path)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            if (_data is null)
            {
                await InitDataAsync().ConfigureAwait(false);
            }

            if (_data is null || !_data.TryGetValue(DataJson.UploadFolder, out var uploadFolder))
            {
                return new(ResultEnum.Error, null, "Upload folder not found");
            }

            var url = Path.Combine(uploadFolder, path);

            return new(ResultEnum.Success, new(url), string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while getting upload folder from GitHub ===");

            return new(ResultEnum.Error, null, "Error while getting upload folder from GitHub");
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }


    /// <summary>
    ///     Initializes the cached data dictionary by downloading data.json from GitHub.
    /// </summary>
    private async Task InitDataAsync()
    {
        using var httpClient = _httpClientFactory.CreateClient(HttpClientEnum.GitHub.GetDescription());
        using var response = await httpClient.GetStreamAsync(CommonConstants.DataJsonUrl).ConfigureAwait(false);

        _data = await JsonSerializer.DeserializeAsync(response, DataJsonModelContext.Default.DictionaryStringString).ConfigureAwait(false)
             ?? throw new FormatException("Error while deserializing meta.json");
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
