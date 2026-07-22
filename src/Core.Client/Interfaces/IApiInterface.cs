using Core.All;
using Core.All.Enums;
using Core.All.Serializable.Addon;
using Core.All.Serializable.Downloadable;

namespace Core.Client.Interfaces;

/// <summary>
///     Defines the contract for interacting with the backend API for releases, addons, metadata, and ratings.
/// </summary>
public interface IApiInterface
{
    /// <summary>
    ///     Adds an addon to the local or remote database.
    /// </summary>
    /// <param name="addonJson">The addon manifest.</param>
    /// <param name="downloadableAddonJson">The downloadable addon metadata.</param>
    /// <returns>true if the addon was added successfully; otherwise, false.</returns>
    Task<bool> AddAddonToDatabaseAsync(AddonManifestJsonModel addonJson, DownloadableAddonJsonModel downloadableAddonJson);

    /// <summary>
    ///     Changes the score for a given addon.
    /// </summary>
    /// <param name="addonId">The addon identifier.</param>
    /// <param name="score">The score delta.</param>
    /// <param name="isNew">Whether this is a new rating.</param>
    /// <returns>The updated score, or null on failure.</returns>
    Task<decimal?> ChangeScoreAsync(string addonId, sbyte score, bool isNew);

    /// <summary>
    ///     Retrieves the list of downloadable addons for the specified game.
    /// </summary>
    /// <param name="gameEnum">The game to filter addons by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of addon models, or null on failure.</returns>
    Task<List<DownloadableAddonJsonModel>?> GetAddonsAsync(GameEnum gameEnum, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the latest application release for self-update.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The latest release, or null if not available.</returns>
    Task<GeneralReleaseJsonModel?> GetLatestAppReleaseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the latest release for the specified port.
    /// </summary>
    /// <param name="portEnum">The port identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The latest release, or null if not available.</returns>
    Task<GeneralReleaseJsonModel?> GetLatestPortReleaseAsync(PortEnum portEnum, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the latest release for the specified tool.
    /// </summary>
    /// <param name="toolEnum">The tool identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The latest release, or null if not available.</returns>
    Task<GeneralReleaseJsonModel?> GetLatestToolReleaseAsync(ToolEnum toolEnum, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves all addon ratings from the server.
    /// </summary>
    /// <returns>A dictionary of addon ratings, or null on failure.</returns>
    Task<Dictionary<string, decimal>?> GetRatingsAsync();

    /// <summary>
    ///     Retrieves a signed upload URL for the specified file path.
    /// </summary>
    /// <param name="path">The relative file path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the signed URL.</returns>
    Task<Result<Uri?>> GetSignedUrlAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the upload folder path from the server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The upload folder path, or null on failure.</returns>
    Task<string?> GetUploadFolderAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves addon metadata from the server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of addon manifests, or null on failure.</returns>
    Task<List<AddonManifestJsonModel>?> GetMetadataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Increments the install count for the specified addon.
    /// </summary>
    /// <param name="addonId">The addon identifier.</param>
    /// <returns>true if the count was incremented; otherwise, false.</returns>
    Task<bool> IncreaseNumberOfInstallsAsync(string addonId);
}
