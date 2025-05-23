using Common.Client.Helpers;
using Common.Client.Interfaces;
using Common.Common.Interfaces;
using Common.Common.Providers;
using Common.Common.Serializable.Downloadable;
using Common.Enums;
using Common.Helpers;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Common.Client.Api;

public sealed class GitHubApiInterface : IApiInterface
{
    private readonly IRetriever<Dictionary<PortEnum, GeneralReleaseJsonModel>?> _portsReleasesRetriever;
    private readonly RepoAppReleasesRetriever _appReleasesProvider;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _semaphore = new(1);

    private Dictionary<GameEnum, List<DownloadableAddonJsonModel>>? _addonsJson = null;


    public GitHubApiInterface(
        IRetriever<Dictionary<PortEnum, GeneralReleaseJsonModel>?> portsReleasesProvider,
        RepoAppReleasesRetriever appReleasesProvider,
        HttpClient httpClient,
        ILogger logger
        )
    {
        _portsReleasesRetriever = portsReleasesProvider;
        _appReleasesProvider = appReleasesProvider;
        _httpClient = httpClient;
        _logger = logger;
    }


    public async Task<List<DownloadableAddonJsonModel>?> GetAddonsAsync(GameEnum gameEnum)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            if (_addonsJson is null)
            {
                string? addons;

                if (ClientProperties.IsOfflineMode)
                {
                    if (ClientProperties.PathToLocalAddonsJson is null)
                    {
                        return null;
                    }

                    addons = File.ReadAllText(ClientProperties.PathToLocalAddonsJson);
                }
                else
                {
                    addons = await _httpClient.GetStringAsync(Consts.AddonsJsonUrl).ConfigureAwait(false);
                }

                _addonsJson = JsonSerializer.Deserialize(addons, DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel);

                if (_addonsJson is null)
                {
                    ThrowHelper.ThrowArgumentNullException();
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
        if (ClientProperties.IsOfflineMode)
        {
            return null;
        }

        await _appReleasesProvider.GetLatestVersionAsync(ClientProperties.IsDeveloperMode).ConfigureAwait(false);

        var result = _appReleasesProvider.AppRelease[CommonProperties.OSEnum];

        return result;
    }

    public async Task<Dictionary<PortEnum, GeneralReleaseJsonModel>?> GetLatestPortsReleasesAsync()
    {
        if (ClientProperties.IsOfflineMode)
        {
            return [];
        }

        var result = await _portsReleasesRetriever.RetrieveAsync().ConfigureAwait(false);

        return result;
    }

    public Task<bool> AddAddonToDatabaseAsync(DownloadableAddonJsonModel addon)
    {
        if (ClientProperties.PathToLocalAddonsJson is null)
        {
            ThrowHelper.ThrowFormatException("Can't find local addons.json");
            return Task.FromResult(false);
        }

        var addonsJson = File.ReadAllText(ClientProperties.PathToLocalAddonsJson);
        var addons = JsonSerializer.Deserialize(addonsJson, DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel);

        if (addons is null)
        {
            ThrowHelper.ThrowFormatException("Error while deserializing addons.json");
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


    #region Not Implemented

    public Task<string?> GetSignedUrlAsync(string path) => Task.FromResult<string?>(null);

    public Task<decimal?> ChangeScoreAsync(string addonId, sbyte score, bool isNew) => Task.FromResult<decimal?>(null);

    public Task<Dictionary<ToolEnum, GeneralReleaseJsonModel>?> GetLatestToolsReleasesAsync() => Task.FromResult<Dictionary<ToolEnum, GeneralReleaseJsonModel>?>(null);

    public Task<Dictionary<string, decimal>?> GetRatingsAsync() => Task.FromResult<Dictionary<string, decimal>?>(null);

    public Task<bool> IncreaseNumberOfInstallsAsync(string addonId) => Task.FromResult(false);

    #endregion
}
