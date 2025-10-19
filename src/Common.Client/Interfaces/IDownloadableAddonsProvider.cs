using System.Collections.Immutable;
using Common.All.Enums;
using Common.All.Helpers;
using Common.All.Serializable.Downloadable;

namespace Common.Client.Interfaces;

public interface IDownloadableAddonsProvider
{
    event AddonChanged AddonDownloadedEvent;

    /// <summary>
    /// Download progress
    /// </summary>
    Progress<float> Progress { get; }

    /// <summary>
    /// Download addon
    /// </summary>
    /// <param name="addon">Addon</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DownloadAddonAsync(DownloadableAddonJsonModel addon, CancellationToken cancellationToken);

    /// <summary>
    /// Get a list of downloadable addons
    /// </summary>
    /// <param name="addonType">Addon type</param>
    ImmutableList<DownloadableAddonJsonModel> GetDownloadableAddons(AddonTypeEnum addonType);

    /// <summary>
    /// Create downloadable addons cache
    /// </summary>
    /// <param name="createNew">Drop existing cache and create new</param>
    /// <returns>Is cache created successfully</returns>
    Task<bool> CreateCacheAsync(bool createNew);
}