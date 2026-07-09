using System.Security.Cryptography;
using Core.All;
using Core.All.Serializable.Addon;
using Core.All.Serializable.Downloadable;
using Core.Client.Interfaces;
using Microsoft.Extensions.Logging;

namespace Core.Client.Tools;

/// <summary>
///     Manages the addition of addon files to the local or remote addon database.
/// </summary>
public sealed class AddonsDatabaseManager
{
    private readonly IApiInterface _apiInterface;

    private readonly ILogger<AddonsDatabaseManager> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonsDatabaseManager" /> class.
    /// </summary>
    /// <param name="apiInterface">The API interface for database operations.</param>
    /// <param name="logger">Logger instance.</param>
    public AddonsDatabaseManager(IApiInterface apiInterface, ILogger<AddonsDatabaseManager> logger)
    {
        _apiInterface = apiInterface;
        _logger = logger;
    }

    /// <summary>
    ///     Adds an addon file to the database by computing its hashes and uploading metadata.
    /// </summary>
    /// <param name="pathToFile">Absolute path to the addon file.</param>
    /// <param name="downloadUrl">The download URL for the addon.</param>
    /// <param name="manifest">The addon manifest.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<Result> AddToDatabaseAsync(string pathToFile, Uri downloadUrl, AddonManifestJsonModel manifest)
    {
        var downloadAddonEntity = await GetDownloadableAddonDtoAsync(pathToFile, downloadUrl, manifest).ConfigureAwait(false);
        var dbResult = await _apiInterface.AddAddonToDatabaseAsync(manifest, downloadAddonEntity).ConfigureAwait(false);

        return new(dbResult ? ResultEnum.Success : ResultEnum.Error, dbResult ? string.Empty : "Error while adding addon to the database.");
    }

    /// <summary>
    ///     Creates a <see cref="DownloadableAddonJsonModel" /> DTO by computing file hashes.
    /// </summary>
    /// <param name="pathToFile">Absolute path to the addon file.</param>
    /// <param name="downloadUrl">The download URL for the addon.</param>
    /// <param name="manifest">The addon manifest.</param>
    /// <returns>A populated <see cref="DownloadableAddonJsonModel" /> instance.</returns>
    private static async Task<DownloadableAddonJsonModel> GetDownloadableAddonDtoAsync(string pathToFile, Uri downloadUrl, AddonManifestJsonModel manifest)
    {
        FileInfo fileInfo = new(pathToFile);
        using var fileStream = File.OpenRead(pathToFile);

        var sha = await SHA256.HashDataAsync(fileStream, CancellationToken.None).ConfigureAwait(false);
        fileStream.Position = 0;
        var md5 = await MD5.HashDataAsync(fileStream, CancellationToken.None).ConfigureAwait(false);

        return new DownloadableAddonJsonModel
        {
            Id = manifest.Id,
            Title = manifest.Title,
            DownloadUrl = downloadUrl,
            Game = manifest.SupportedGame.Game,
            AddonType = manifest.AddonType,
            Version = manifest.Version,
            Description = manifest.Description,
            Author = manifest.Author,
            FileSize = fileInfo.Length,
            Dependencies = manifest.Dependencies?.Addons?.Select(d => d.Id)?.ToList(),
            UpdateDate = DateTime.UtcNow,
            MD5 = Convert.ToHexString(md5),
            Sha256 = Convert.ToHexString(sha)
        };
    }
}
