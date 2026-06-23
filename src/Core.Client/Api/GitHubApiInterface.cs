using System.Text.Json;
using Core.All;
using Core.All.Enums;
using Core.All.Helpers;
using Core.All.Providers;
using Core.All.Serializable;
using Core.All.Serializable.Addon;
using Core.All.Serializable.Downloadable;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Microsoft.Extensions.Logging;

namespace Core.Client.Api;

public sealed class GitHubApiInterface : IApiInterface
{
    private readonly ReleaseProvider<PortEnum> _portsReleasesProvider;
    private readonly ReleaseProvider<ToolEnum> _toolsReleasesProvider;
    private readonly RepoAppReleasesProvider _appReleasesProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GitHubApiInterface> _logger;
    private readonly SemaphoreSlim _semaphore = new(1);

    private Dictionary<GameEnum, List<DownloadableAddonJsonModel>>? _addonsJson;
    private Dictionary<string, string>? _data;


    public GitHubApiInterface(
        ReleaseProvider<PortEnum> portsReleasesProvider,
        ReleaseProvider<ToolEnum> toolsReleasesRetriever,
        RepoAppReleasesProvider appReleasesProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<GitHubApiInterface> logger
        )
    {
        _portsReleasesProvider = portsReleasesProvider;
        _toolsReleasesProvider = toolsReleasesRetriever;
        _appReleasesProvider = appReleasesProvider;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }


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

    public async Task<GeneralReleaseJsonModel?> GetLatestAppReleaseAsync()
    {
        var result = await _appReleasesProvider.GetLatestReleaseAsync(ClientProperties.IsDeveloperMode).ConfigureAwait(false);

        if (result?.TryGetValue(CommonProperties.OSEnum, out var release) is true)
        {
            return release;
        }

        return null;
    }

    public async Task<GeneralReleaseJsonModel?> GetLatestPortReleaseAsync(PortEnum portEnum)
    {
        var result = await _portsReleasesProvider.GetLatestReleaseAsync(portEnum).ConfigureAwait(false);

        if (result?.TryGetValue(CommonProperties.OSEnum, out var release) is true)
        {
            return release;
        }

        return null;
    }

    public async Task<GeneralReleaseJsonModel?> GetLatestToolReleaseAsync(ToolEnum toolEnum)
    {
        var result = await _toolsReleasesProvider.GetLatestReleaseAsync(toolEnum).ConfigureAwait(false);

        if (result?.TryGetValue(CommonProperties.OSEnum, out var release) is true)
        {
            return release;
        }

        return null;
    }

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

        var newAddonsJson = JsonSerializer.Serialize(addons, DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel);
        await File.WriteAllTextAsync(ClientProperties.PathToLocalAddonsJson, newAddonsJson).ConfigureAwait(false);

        List<AddonManifestJsonModel>? manifests;

        using (var manifestsJson = File.OpenRead(ClientProperties.PathToLocalManifestsJson))
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

        var newManifestsJson = JsonSerializer.Serialize(manifests, AddonManifestJsonContext.Default.ListAddonManifestJsonModel);
        await File.WriteAllTextAsync(ClientProperties.PathToLocalManifestsJson, newManifestsJson).ConfigureAwait(false);

        return true;
    }

    public async Task<string?> GetUploadFolderAsync()
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            if (_data is null)
            {
                await InitDataAsync().ConfigureAwait(false);
            }

            return _data!.TryGetValue(DataJson.UploadFolder, out var uploadFolder) ? uploadFolder : null;
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


    private async Task InitDataAsync()
    {
        using var httpClient = _httpClientFactory.CreateClient(HttpClientEnum.GitHub.GetDescription());
        using var response = await httpClient.GetStreamAsync(CommonConstants.DataJsonUrl).ConfigureAwait(false);

        _data = await JsonSerializer.DeserializeAsync(response, DataJsonModelContext.Default.DictionaryStringString).ConfigureAwait(false)
             ?? throw new FormatException("Error while deserializing meta.json");
    }

    public async Task<Result<Uri?>> GetSignedUrlAsync(string path)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            if (_data is null)
            {
                await InitDataAsync().ConfigureAwait(false);
            }

            _ = _data!.TryGetValue(DataJson.UploadFolder, out var uploadFolder) ? uploadFolder : null;

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


    #region Not Implemented

    public Task<decimal?> ChangeScoreAsync(string addonId, sbyte score, bool isNew) => Task.FromResult<decimal?>(null);

    public Task<Dictionary<string, decimal>?> GetRatingsAsync() => Task.FromResult<Dictionary<string, decimal>?>(null);

    public Task<bool> IncreaseNumberOfInstallsAsync(string addonId) => Task.FromResult(false);

    #endregion
}
