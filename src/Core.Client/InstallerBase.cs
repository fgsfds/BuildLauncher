using System.Security.Cryptography;
using Core.All.Serializable.Downloadable;
using Core.Client.Interfaces;
using Core.Client.Tools;
using Microsoft.Extensions.Logging;

namespace Core.Client;

/// <summary>
///     Provides a base implementation for installing and uninstalling ports and tools.
/// </summary>
/// <typeparam name="T">The type of installable item.</typeparam>
public abstract class InstallerBase<T>
    where T : IInstallable
{
    private readonly ArchiveTools _archiveTools;

    private readonly FilesDownloader _filesDownloader;

    /// <summary>
    ///     The installable instance.
    /// </summary>
    protected readonly T _instance;

    private readonly ILogger _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstallerBase{T}" /> class.
    /// </summary>
    /// <param name="instance">The installable instance.</param>
    /// <param name="filesDownloader">The file downloader service.</param>
    /// <param name="archiveTools">The archive extraction utility.</param>
    /// <param name="logger">Logger instance.</param>
    protected InstallerBase(
        T instance,
        FilesDownloader filesDownloader,
        ArchiveTools archiveTools,
        ILogger logger
        )
    {
        _instance = instance;
        _filesDownloader = filesDownloader;
        _archiveTools = archiveTools;
        _logger = logger;
    }

    /// <summary>
    ///     Installation progress.
    /// </summary>
    public Progress<float> Progress { get; init; } = new();

    /// <summary>
    ///     Verifies the SHA-256 hash of a downloaded file against the expected value.
    /// </summary>
    /// <param name="filePath">Path to the downloaded file.</param>
    /// <param name="hashStr">Expected hash string, optionally prefixed with "sha256:".</param>
    /// <returns>true if the hash matches or hashStr is null; otherwise, false.</returns>
    private static async Task<bool> CheckFileHashAsync(
        string filePath,
        string? hashStr
        )
    {
        if (hashStr is null)
        {
            return true;
        }

        const string Sha = "sha256:";

        if (hashStr.StartsWith(Sha))
        {
            hashStr = hashStr[Sha.Length..];
        }

        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
        using var sha256 = SHA256.Create();

        var hashBytes = sha256.ComputeHash(fileStream);
        var localFileHash = Convert.ToHexString(hashBytes);

        return hashStr.Equals(localFileHash, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Performs post install actions.
    /// </summary>
    /// <param name="filePath">Path to the downloaded file.</param>
    protected abstract void PostInstall(string filePath);

    /// <summary>
    ///     Backups required files.
    /// </summary>
    protected abstract void Backup();

    /// <summary>
    ///     Uninstalls port/tool.
    /// </summary>
    public abstract void Uninstall();

    /// <summary>
    ///     Returns latest release.
    /// </summary>
    public abstract Task<GeneralReleaseJsonModel?> GetRelease();

    /// <summary>
    ///     Installs port/tool.
    /// </summary>
    public async Task<bool> InstallAsync()
    {
        try
        {
            _filesDownloader.ProgressChanged += OnProgressChanged;
            _archiveTools.ProgressChanged += OnProgressChanged;

            var release = await GetRelease().ConfigureAwait(false);

            if (release?.DownloadUrl is null)
            {
                return false;
            }

            var filePath = Path.GetFileName(release.DownloadUrl.ToString());

            var isDownloaded = await _filesDownloader.DownloadFileAsync(release.DownloadUrl, filePath, CancellationToken.None).ConfigureAwait(false);

            if (!isDownloaded)
            {
                return false;
            }

            var isHashCorrect = await CheckFileHashAsync(filePath, release.Hash).ConfigureAwait(false);

            if (!isHashCorrect)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return false;
            }

            Backup();

            await _archiveTools.UnpackArchiveAsync(filePath, _instance.InstallFolderPath).ConfigureAwait(false);

            File.Delete(filePath);

            await File.WriteAllTextAsync(Path.Combine(_instance.InstallFolderPath, "version"), release.Version).ConfigureAwait(false);

            PostInstall(filePath);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error while installing {_instance.ToString}.");
            Uninstall();

            throw;
        }
        finally
        {
            _filesDownloader.ProgressChanged -= OnProgressChanged;
            _archiveTools.ProgressChanged -= OnProgressChanged;
        }
    }

    /// <summary>
    ///     Handles progress change events from the downloader and archive tools.
    /// </summary>
    /// <param name="_">Unused sender parameter.</param>
    /// <param name="e">Progress value.</param>
    protected void OnProgressChanged(object? _, float e) => ((IProgress<float>)Progress).Report(e);
}
