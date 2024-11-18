using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using System.Collections.Immutable;

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
    Task DownloadAddonAsync(IDownloadableAddon addon, CancellationToken cancellationToken);

    /// <summary>
    /// Download addon
    /// </summary>
    /// <param name="addonType">Addon type</param>
    ImmutableList<IDownloadableAddon> GetDownloadableAddons(AddonTypeEnum addonType);
}