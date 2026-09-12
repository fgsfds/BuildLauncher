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
    /// <param name="portsReleasesProviderBase">
    ///     Provider for port releases.
    /// </param>
    /// <param name="toolsReleasesRetriever">
    ///     Provider for tool releases.
    /// </param>
    /// <param name="appRepoReleasesProvider">
    ///     Provider for app self-update releases.
    /// </param>
    /// <param name="httpClientFactory">
    ///     Factory for creating HTTP clients.
    /// </param>
    /// <param name="logger">
    ///     Logger instance.
    /// </param>
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
    public async Task<List<DownloadableAddonJsonModel>?> GetAddonsAsync(GameEnum gameEnum, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_addonsJson is null)
            {
                const int maxRetries = 3;

                for (var attempt = 1; attempt <= maxRetries; attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        using var httpClient = _httpClientFactory.CreateClient(HttpClientEnum.GitHub.GetDescription());
                        await using var response = await httpClient.GetStreamAsync(CommonConstants.AddonsJsonUrl, cancellationToken).ConfigureAwait(false);

                        _addonsJson = await JsonSerializer.DeserializeAsync(
                            response,
                            DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel,
                            cancellationToken
                            ).ConfigureAwait(false);

                        if (_addonsJson is null)
                        {
                            throw new FormatException("Error while deserializing addons.json");
                        }

                        _logger.LogInformation("Downloaded addons.json from {Url} with {Count} games (attempt {Attempt})", CommonConstants.AddonsJsonUrl, _addonsJson.Count, attempt);

                        break;
                    }
                    catch (Exception ex) when ((ex is TaskCanceledException or HttpRequestException) && attempt < maxRetries)
                    {
                        _logger.LogWarning(ex, "Error while getting addons from GitHub (attempt {Attempt}/{MaxRetries})", attempt, maxRetries);
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken).ConfigureAwait(false);
                    }
                }

                if (_addonsJson is null)
                {
                    throw new InvalidOperationException("Failed to download addons after multiple retries");
                }
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
            _logger.LogCritical(ex, "=== Error while getting addons from GitHub ===");

            return null;
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<GeneralReleaseJsonModel?> GetLatestAppReleaseAsync(CancellationToken cancellationToken = default)
    {
        var result = await _appRepoReleasesProvider.GetLatestReleaseAsync(AppReleaseEnum.MainApp, ClientProperties.IsDeveloperMode, cancellationToken).ConfigureAwait(false);

        if (result?.TryGetValue(CommonProperties.OSEnum, out var release) is true)
        {
            _logger.LogInformation("Latest release resolved for app ({OS}): {Version}", release.SupportedOS, release.Version);

            return release;
        }

        _logger.LogWarning("Latest release not found for app ({OS})", CommonProperties.OSEnum);

        return null;
    }

    /// <inheritdoc />
    public async Task<GeneralReleaseJsonModel?> GetLatestPortReleaseAsync(PortEnum portEnum, CancellationToken cancellationToken = default)
    {
        var result = await _portsReleasesProviderBase.GetLatestReleaseAsync(portEnum, false, cancellationToken).ConfigureAwait(false);

        if (result?.TryGetValue(CommonProperties.OSEnum, out var release) is true)
        {
            _logger.LogInformation("Latest release resolved for port {Port} ({OS}): {Version}", portEnum, release.SupportedOS, release.Version);

            return release;
        }

        _logger.LogWarning("Latest release not found for port {Port} ({OS})", portEnum, CommonProperties.OSEnum);

        return null;
    }

    /// <inheritdoc />
    public async Task<GeneralReleaseJsonModel?> GetLatestToolReleaseAsync(ToolEnum toolEnum, CancellationToken cancellationToken = default)
    {
        var result = await _toolsReleasesProviderBase.GetLatestReleaseAsync(toolEnum, false, cancellationToken).ConfigureAwait(false);

        if (result?.TryGetValue(CommonProperties.OSEnum, out var release) is true)
        {
            _logger.LogInformation("Latest release resolved for tool {Tool} ({OS}): {Version}", toolEnum, release.SupportedOS, release.Version);

            return release;
        }

        _logger.LogWarning("Latest release not found for tool {Tool} ({OS})", toolEnum, CommonProperties.OSEnum);

        return null;
    }

    /// <inheritdoc />
    public async Task<bool> AddAddonToDatabaseAsync(AddonManifestJsonModel addonJson, DownloadableAddonJsonModel downloadableAddonJson)
    {
        if (ClientProperties.PathToLocalAddonsJson is null)
        {
            _logger.LogError("Local addons.json path is not set");

            return false;
        }

        if (ClientProperties.PathToLocalManifestsJson is null)
        {
            _logger.LogError("Local manifests.json path is not set");

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

        _logger.LogInformation("Added addon {AddonId} v{Version} to the local database", addonJson.Id, addonJson.Version);

        return true;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, string>?> GetDataJsonAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_data is null)
            {
                await InitDataAsync(cancellationToken).ConfigureAwait(false);
            }

            _logger.LogInformation("Returning data.json with {Count} entries", _data?.Count ?? 0);

            return _data;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while getting data from GitHub ===");

            return null;
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<string?> GetUploadFolderAsync(CancellationToken cancellationToken = default)
    {
        var data = await GetDataJsonAsync(cancellationToken).ConfigureAwait(false);

        if (data?.TryGetValue(DataJson.UploadFolder, out var uploadFolder) == true)
        {
            _logger.LogInformation("Upload folder resolved from data.json: {UploadFolder}", uploadFolder);

            return uploadFolder;
        }

        _logger.LogWarning("Upload folder is missing in data.json");

        return null;
    }

    /// <inheritdoc />
    public async Task<List<AddonManifestJsonModel>?> GetMetadataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();

            using var jsonStream = await httpClient.GetStreamAsync(CommonConstants.ManifestsJsonUrl, cancellationToken).ConfigureAwait(false)
                                ?? throw new FormatException("Error while deserializing manifests.json");

            var meta = await JsonSerializer.DeserializeAsync(
                jsonStream,
                AddonManifestJsonContext.Default.ListAddonManifestJsonModel,
                cancellationToken
                ).ConfigureAwait(false);

            _logger.LogInformation("Downloaded {Count} addon manifests from {Url}", meta?.Count ?? 0, CommonConstants.ManifestsJsonUrl);

            return meta;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while getting manifests from GitHub ===");

            return null;
        }
    }

    /// <inheritdoc />
    public async Task<Result<Uri?>> GetSignedUrlAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await GetDataJsonAsync(cancellationToken).ConfigureAwait(false);

        if (data is null || !data.TryGetValue(DataJson.UploadFolder, out var uploadFolder))
        {
            _logger.LogWarning("Failed to build signed URL for {Path}: upload folder not found", path);

            return new(ResultEnum.Error, null, "Upload folder not found");
        }

        var url = Path.Combine(uploadFolder, path);

        _logger.LogInformation("Built signed URL for {Path}", path);

        return new(ResultEnum.Success, new(url), string.Empty);
    }


    /// <summary>
    ///     Initializes the cached data dictionary by downloading data.json from GitHub.
    /// </summary>
    private async Task InitDataAsync(CancellationToken cancellationToken = default)
    {
        using var httpClient = _httpClientFactory.CreateClient(HttpClientEnum.GitHub.GetDescription());
        using var response = await httpClient.GetStreamAsync(CommonConstants.DataJsonUrl, cancellationToken).ConfigureAwait(false);

        _data = await JsonSerializer.DeserializeAsync(response, DataJsonModelContext.Default.DictionaryStringString, cancellationToken).ConfigureAwait(false)
             ?? throw new FormatException("Error while deserializing meta.json");

        _logger.LogInformation("Downloaded data.json from {Url} with {Count} entries", CommonConstants.DataJsonUrl, _data.Count);
    }


    #region Not Implemented

    /// <inheritdoc />
    public Task<decimal?> ChangeScoreAsync(string addonId, sbyte score, bool isNew)
    {
        _logger.LogDebug("GitHub API does not support score changes; ignoring score change for addon {AddonId}", addonId);

        return Task.FromResult<decimal?>(null);
    }

    /// <inheritdoc />
    public Task<Dictionary<string, decimal>?> GetRatingsAsync()
    {
        _logger.LogDebug("GitHub API does not provide ratings; returning no ratings");

        return Task.FromResult<Dictionary<string, decimal>?>(null);
    }

    /// <inheritdoc />
    public Task<bool> IncreaseNumberOfInstallsAsync(string addonId)
    {
        _logger.LogDebug("GitHub API does not track install counts; ignoring install count for addon {AddonId}", addonId);

        return Task.FromResult(false);
    }

    #endregion
}
