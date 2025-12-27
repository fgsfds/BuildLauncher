using Common.All.Enums;
using Common.All.Serializable.Downloadable;

namespace Common.Client.Interfaces;

public interface IApiInterface
{
    Task<bool> AddAddonToDatabaseAsync(DownloadableAddonJsonModel addon);
    Task<decimal?> ChangeScoreAsync(string addonId, sbyte score, bool isNew);
    Task<List<DownloadableAddonJsonModel>?> GetAddonsAsync(GameEnum gameEnum);
    Task<GeneralReleaseJsonModel?> GetLatestAppReleaseAsync();
    Task<GeneralReleaseJsonModel?> GetLatestPortReleaseAsync(PortEnum portEnum);
    Task<GeneralReleaseJsonModel?> GetLatestToolReleaseAsync(ToolEnum toolEnum);
    Task<Dictionary<string, decimal>?> GetRatingsAsync();
    Task<string?> GetSignedUrlAsync(string path);
    Task<string?> GetUploadFolder();
    Task<bool> IncreaseNumberOfInstallsAsync(string addonId);
}