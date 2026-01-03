using Common.All;
using Common.All.Enums;
using Common.All.Serializable.Downloadable;

namespace Common.Client.Interfaces;

public interface IApiInterface
{
    Task<bool> AddAddonToDatabaseAsync(DownloadableAddonJsonModel addon);
    Task<decimal?> ChangeScoreAsync(string addonId, sbyte score, bool isNew);
    Task<List<DownloadableAddonJsonModel>?> GetAddonsAsync(GameEnum gameEnum);
    Task<GeneralRelease?> GetLatestAppReleaseAsync();
    Task<GeneralRelease?> GetLatestPortReleaseAsync(PortEnum portEnum);
    Task<GeneralRelease?> GetLatestToolReleaseAsync(ToolEnum toolEnum);
    Task<Dictionary<string, decimal>?> GetRatingsAsync();
    Task<string?> GetSignedUrlAsync(string path);
    Task<string?> GetUploadFolder();
    Task<bool> IncreaseNumberOfInstallsAsync(string addonId);
}