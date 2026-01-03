using System.Text.Json;
using Common.All;
using Common.All.Enums;
using Common.All.Helpers;
using Common.All.Interfaces;
using Common.All.Providers;
using Common.All.Serializable;
using Common.All.Serializable.Downloadable;
using Common.Client.Helpers;
using Common.Client.Interfaces;
using Microsoft.Extensions.Logging;

namespace Common.Client.Api;

public sealed class GitHubApiInterface : IApiInterface
{
    private readonly IReleaseProvider<PortEnum> _portsReleasesProvider;
    private readonly IReleaseProvider<ToolEnum> _toolsReleasesProvider;
    private readonly RepoAppReleasesProvider _appReleasesProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _semaphore = new(1);

    private Dictionary<GameEnum, List<DownloadableAddonJsonModel>>? _addonsJson;
    private Dictionary<string, string>? _data;


    public GitHubApiInterface(
        IReleaseProvider<PortEnum> portsReleasesProvider,
        IReleaseProvider<ToolEnum> toolsReleasesRetriever,
        RepoAppReleasesProvider appReleasesProvider,
        IHttpClientFactory httpClientFactory,
        ILogger logger
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
                using var response = await httpClient.GetAsync(Consts.AddonsJsonUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                _ = response.EnsureSuccessStatusCode();

                var addons = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

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

    public async Task<GeneralRelease?> GetLatestAppReleaseAsync()
    {
        var result = await _appReleasesProvider.GetLatestReleaseAsync(ClientProperties.IsDeveloperMode).ConfigureAwait(false);

        if (result?.TryGetValue(CommonProperties.OSEnum, out var release) is true)
        {
            return release;
        }

        return null;
    }

    public async Task<GeneralRelease?> GetLatestPortReleaseAsync(PortEnum portEnum)
    {
        var result = await _portsReleasesProvider.GetLatestReleaseAsync(portEnum).ConfigureAwait(false);

        if (result?.TryGetValue(CommonProperties.OSEnum, out var release) is true)
        {
            return release;
        }

        return null;
    }

    public async Task<GeneralRelease?> GetLatestToolReleaseAsync(ToolEnum toolEnum)
    {
        var result = await _toolsReleasesProvider.GetLatestReleaseAsync(toolEnum).ConfigureAwait(false);

        if (result?.TryGetValue(CommonProperties.OSEnum, out var release) is true)
        {
            return release;
        }

        return null;
    }

    public Task<bool> AddAddonToDatabaseAsync(DownloadableAddonJsonModel addon)
    {
        if (ClientProperties.PathToLocalAddonsJson is null)
        {
            throw new FormatException("Can't find local addons.json");
        }

        var addonsJson = File.ReadAllText(ClientProperties.PathToLocalAddonsJson);
        var addons = JsonSerializer.Deserialize(addonsJson, DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel);

        if (addons is null)
        {
            throw new FormatException("Error while deserializing addons.json");
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

    public async Task<string?> GetUploadFolder()
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            if (_data is null)
            {
                await InitData().ConfigureAwait(false);
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


    private async Task InitData()
    {
        using var httpClient = _httpClientFactory.CreateClient(HttpClientEnum.GitHub.GetDescription());
        using var response = await httpClient.GetAsync(Consts.DataJsonUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        _data = JsonSerializer.Deserialize(data, DataJsonModelContext.Default.DictionaryStringString);

        if (_data is null)
        {
            throw new ArgumentNullException();
        }
    }


    #region Not Implemented

    public Task<string?> GetSignedUrlAsync(string path) => Task.FromResult<string?>(null);

    public Task<decimal?> ChangeScoreAsync(string addonId, sbyte score, bool isNew) => Task.FromResult<decimal?>(null);

    public Task<Dictionary<string, decimal>?> GetRatingsAsync() => Task.FromResult<Dictionary<string, decimal>?>(null);

    public Task<bool> IncreaseNumberOfInstallsAsync(string addonId) => Task.FromResult(false);

    #endregion
}
