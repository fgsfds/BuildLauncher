using Core.All;
using Core.All.Enums;
using Core.All.Serializable.Addon;
using Core.All.Serializable.Downloadable;

namespace Core.Client.Interfaces;

public interface IApiInterface
{
    Task<bool> AddAddonToDatabaseAsync(AddonJsonModel addonJson, DownloadableAddonJsonModel downloadableAddonJson);
    Task<decimal?> ChangeScoreAsync(string addonId, sbyte score, bool isNew);
    Task<List<DownloadableAddonJsonModel>?> GetAddonsAsync(GameEnum gameEnum);
    Task<GeneralReleaseJsonModel?> GetLatestAppReleaseAsync();
    Task<GeneralReleaseJsonModel?> GetLatestPortReleaseAsync(PortEnum portEnum);
    Task<GeneralReleaseJsonModel?> GetLatestToolReleaseAsync(ToolEnum toolEnum);
    Task<Dictionary<string, decimal>?> GetRatingsAsync();
    Task<Result<Uri?>> GetSignedUrlAsync(string path);
    Task<string?> GetUploadFolderAsync();
    Task<List<AddonJsonModel>?> GetMetadataAsync();
    Task<bool> IncreaseNumberOfInstallsAsync(string addonId);
}
