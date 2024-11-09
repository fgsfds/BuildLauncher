using Common.Client.Interfaces;
using Common.Common.Helpers;
using Common.Common.Providers;
using Common.Entities;
using Common.Enums;
using Common.Helpers;
using CommunityToolkit.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Client.Api;

public sealed class GitHubApiInterface : IApiInterface
{
    private readonly PortsReleasesProvider _portsReleasesProvider;
    private readonly AppReleasesProvider _appReleasesProvider;
    private readonly HttpClient _httpClient;

    private static Dictionary<GameEnum, List<DownloadableAddonEntity>>? _addonsJson = null;
    private SemaphoreSlim _semaphore = new(1);


    public GitHubApiInterface(
        PortsReleasesProvider portsReleasesProvider,
        AppReleasesProvider appReleasesProvider,
        HttpClient httpClient
        )
    {
        _portsReleasesProvider = portsReleasesProvider;
        _appReleasesProvider = appReleasesProvider;
        _httpClient = httpClient;
    }


    public async Task<List<DownloadableAddonEntity>?> GetAddonsAsync(GameEnum gameEnum)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            if (_addonsJson is null)
            {
                var addons = await _httpClient.GetStringAsync(Consts.AddonsJsonUrl).ConfigureAwait(false);

                JsonSerializerOptions options = new();
                options.Converters.Add(new JsonStringEnumConverter<AddonTypeEnum>());
                options.Converters.Add(new JsonStringEnumConverter<GameEnum>());
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

                _addonsJson = JsonSerializer.Deserialize<Dictionary<GameEnum, List<DownloadableAddonEntity>>>(addons, options);

                if (_addonsJson is null)
                {
                    ThrowHelper.ThrowArgumentNullException();
                }

                return _addonsJson.TryGetValue(gameEnum, out var result) ? result : null;
            }
            else
            {
                return _addonsJson.TryGetValue(gameEnum, out var result) ? result : null;
            }
        }
        catch
        {
            return [];
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }

    public async Task<GeneralReleaseEntity?> GetLatestAppReleaseAsync()
    {
        await _appReleasesProvider.GetLatestVersionAsync().ConfigureAwait(false);

        var result = _appReleasesProvider.AppRelease[CommonProperties.OSEnum];

        return result;
    }

    public async Task<Dictionary<PortEnum, GeneralReleaseEntity>?> GetLatestPortsReleasesAsync()
    {
        await _portsReleasesProvider.GetLatestReleasesAsync().ConfigureAwait(false);

        return CommonProperties.OSEnum switch
        {
            OSEnum.Windows => _portsReleasesProvider.WindowsReleases,
            OSEnum.Linux => _portsReleasesProvider.LinuxReleases,
            _ => ThrowHelper.ThrowNotSupportedException<Dictionary<PortEnum, GeneralReleaseEntity>?>(CommonProperties.OSEnum.ToString())
        };
    }

    public Task<string?> GetSignedUrlAsync(string path)
    {
        return Task.FromResult<string?>(null);
    }


    #region Not Implemented

    public Task<bool> AddAddonToDatabaseAsync(AddonsJsonEntity addon) => Task.FromResult(false);

    public Task<decimal?> ChangeScoreAsync(string addonId, sbyte score, bool isNew) => Task.FromResult<decimal?>(null);

    public Task<Dictionary<ToolEnum, GeneralReleaseEntity>?> GetLatestToolsReleasesAsync() => Task.FromResult<Dictionary<ToolEnum, GeneralReleaseEntity>?>(null);

    public Task<Dictionary<string, decimal>?> GetRatingsAsync() => Task.FromResult<Dictionary<string, decimal>?>(null);

    public Task<bool> IncreaseNumberOfInstallsAsync(string addonId) => Task.FromResult(false);

    #endregion
}
