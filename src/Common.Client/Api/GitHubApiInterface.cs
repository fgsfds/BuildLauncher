using Common.Client.Interfaces;
using Common.Common.Helpers;
using Common.Common.Providers;
using Common.Entities;
using Common.Enums;
using CommunityToolkit.Diagnostics;

namespace Common.Client.Api;

public sealed class GitHubApiInterface : IApiInterface
{
    private readonly PortsReleasesProvider _portsReleasesProvider;
    private readonly AppReleasesProvider _appReleasesProvider;


    public GitHubApiInterface(
        PortsReleasesProvider portsReleasesProvider,
        AppReleasesProvider appReleasesProvider
        )
    {
        _portsReleasesProvider = portsReleasesProvider;
        _appReleasesProvider = appReleasesProvider;
    }


    public Task<List<DownloadableAddonEntity>?> GetAddonsAsync(GameEnum gameEnum)
    {
        return Task.FromResult<List<DownloadableAddonEntity>?>(null);
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
