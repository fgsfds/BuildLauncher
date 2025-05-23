using Common.Common.Serializable.Downloadable;
using Common.Enums;

namespace Common.Client.Interfaces;

public interface IApiInterface
{
    Task<bool> AddAddonToDatabaseAsync(DownloadableAddonJsonModel addon);
    Task<decimal?> ChangeScoreAsync(string addonId, sbyte score, bool isNew);
    Task<List<DownloadableAddonJsonModel>?> GetAddonsAsync(GameEnum gameEnum);
    Task<GeneralReleaseJsonModel?> GetLatestAppReleaseAsync();
    Task<Dictionary<PortEnum, GeneralReleaseJsonModel>?> GetLatestPortsReleasesAsync();
    Task<Dictionary<ToolEnum, GeneralReleaseJsonModel>?> GetLatestToolsReleasesAsync();
    Task<Dictionary<string, decimal>?> GetRatingsAsync();
    Task<string?> GetSignedUrlAsync(string path);
    Task<bool> IncreaseNumberOfInstallsAsync(string addonId);
}