using Common.Entities;
using Common.Enums;

namespace Common.Client.Api;

public interface IApiInterface
{
    Task<bool> AddAddonToDatabaseAsync(AddonsJsonEntity addon);
    Task<decimal?> ChangeScoreAsync(string addonId, sbyte score, bool isNew);
    Task<List<DownloadableAddonEntity>?> GetAddonsAsync(GameEnum gameEnum);
    Task<GeneralReleaseEntity?> GetLatestAppReleaseAsync();
    Task<Dictionary<PortEnum, GeneralReleaseEntity>?> GetLatestPortsReleasesAsync();
    Task<Dictionary<ToolEnum, GeneralReleaseEntity>?> GetLatestToolsReleasesAsync();
    Task<Dictionary<string, decimal>?> GetRatingsAsync();
    Task<string?> GetSignedUrlAsync(string path);
    Task<bool> IncreaseNumberOfInstallsAsync(string addonId);
}