using Common.Client.Interfaces;
using Common.Entities;
using Common.Enums;

namespace Common.Client.Api;

public sealed class GitHubApiInterface : IApiInterface
{
    private readonly HttpClient _httpClient;
    private readonly IConfigProvider _config;


    public GitHubApiInterface(
        IConfigProvider config,
        HttpClient httpClient
        )
    {
        _config = config;
        _httpClient = httpClient;
    }


    public Task<List<DownloadableAddonEntity>?> GetAddonsAsync(GameEnum gameEnum)
    {
        return Task.FromResult<List<DownloadableAddonEntity>?>(null);
    }

    public Task<GeneralReleaseEntity?> GetLatestAppReleaseAsync()
    {
        return Task.FromResult<GeneralReleaseEntity?>(null);
    }

    public Task<Dictionary<PortEnum, GeneralReleaseEntity>?> GetLatestPortsReleasesAsync()
    {
        return Task.FromResult<Dictionary<PortEnum, GeneralReleaseEntity>?>(null);
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
