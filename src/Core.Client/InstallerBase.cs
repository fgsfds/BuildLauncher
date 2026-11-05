using Core.Client.Helpers;
using Core.All;
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
    ///     Performs post install actions.
    /// </summary>
    /// <param name="installFolder">The isolated staging directory.</param>
    protected abstract void PostInstall(string installFolder);

    /// <summary>
    ///     Backups required files.
    /// </summary>
    /// <param name="installFolder">The isolated staging directory containing existing files.</param>
    protected abstract void Backup(string installFolder);

    /// <summary>
    ///     Uninstalls port/tool.
    /// </summary>
    public abstract void Uninstall();

    /// <summary>
    ///     Returns latest release.
    /// </summary>
    public abstract Task<GeneralReleaseJsonModel?> GetRelease(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Installs port/tool.
    /// </summary>
    public async Task<Result> InstallAsync(CancellationToken cancellationToken = default)
    {
        string? downloadFolder = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            _filesDownloader.ProgressChanged += OnProgressChanged;
            _archiveTools.ProgressChanged += OnProgressChanged;

            var release = await GetRelease(cancellationToken).ConfigureAwait(false);

            if (release?.DownloadUrl is null)
            {
                return new(ResultEnum.Error, string.Empty);
            }

            downloadFolder = Path.Combine(Path.GetTempPath(), "BuildLauncher-download-" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(downloadFolder);
            var filePath = Path.Combine(downloadFolder, "release.archive");

            var isDownloaded = await _filesDownloader.DownloadFileAsync(release.DownloadUrl, filePath, cancellationToken).ConfigureAwait(false);

            if (!isDownloaded.IsSuccess)
            {
                return isDownloaded;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var isHashCorrect = await FileHash.MatchesAsync(filePath, release.Hash, cancellationToken).ConfigureAwait(false);

            if (!isHashCorrect)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return new(ResultEnum.HashError, string.Empty);
            }

            cancellationToken.ThrowIfCancellationRequested();

            using var replacement = new DirectoryReplacement(_instance.InstallFolderPath, _logger);
            replacement.CopyExisting(cancellationToken);
            Backup(replacement.StagingPath);
            await _archiveTools.UnpackArchiveAsync(filePath, replacement.StagingPath, cancellationToken).ConfigureAwait(false);
            PostInstall(replacement.StagingPath);
            await File.WriteAllTextAsync(Path.Combine(replacement.StagingPath, "version"), release.Version, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            replacement.Commit();

            return new(ResultEnum.Success, string.Empty);
        }
        catch (OperationCanceledException)
        {
            return new(ResultEnum.Cancelled, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error while installing {_instance.ToString()}.");

            return new(ResultEnum.Error, ex.Message);
        }
        finally
        {
            _filesDownloader.ProgressChanged -= OnProgressChanged;
            _archiveTools.ProgressChanged -= OnProgressChanged;
            try
            {
                if (downloadFolder is not null)
                {
                    Directory.Delete(downloadFolder, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not remove download directory {Path}.", downloadFolder);
            }
        }
    }

    /// <summary>
    ///     Handles progress change events from the downloader and archive tools.
    /// </summary>
    /// <param name="_">Unused sender parameter.</param>
    /// <param name="e">Progress value.</param>
    private void OnProgressChanged(object? _, float e) => ((IProgress<float>)Progress).Report(e);
}
