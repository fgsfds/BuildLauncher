using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Core.All.Serializable.Downloadable;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Core.Client.Tools;
using Microsoft.Extensions.Logging;

namespace Core.Client;

/// <summary>
///     Handles the application self-update process including downloading releases and installing updates.
/// </summary>
public sealed class AppUpdateInstaller
{
    private readonly IApiInterface _apiInterface;

    private readonly FilesDownloader _filesDownloader;

    private readonly ILogger<AppUpdateInstaller> _logger;

    /// <summary>
    ///     Latest update release info.
    /// </summary>
    private GeneralReleaseJsonModel? _update;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AppUpdateInstaller" /> class.
    /// </summary>
    /// <param name="filesDownloader">The file downloader service.</param>
    /// <param name="apiInterface">The API interface for fetching release metadata.</param>
    /// <param name="logger">Logger instance.</param>
    public AppUpdateInstaller(
        FilesDownloader filesDownloader,
        IApiInterface apiInterface,
        ILogger<AppUpdateInstaller> logger
        )
    {
        _filesDownloader = filesDownloader;
        _apiInterface = apiInterface;
        _logger = logger;
    }

    /// <summary>
    ///     Check GitHub for releases with version higher than current.
    /// </summary>
    /// <param name="currentVersion">Current application version.</param>
    /// <param name="cancellationToken">Cancels the release lookup.</param>
    /// <returns>Whether an update is available, or null when no release was returned.</returns>
    public async Task<bool?> CheckForUpdates(Version currentVersion, CancellationToken cancellationToken = default)
    {
        var release = await _apiInterface.GetLatestAppReleaseAsync(cancellationToken).ConfigureAwait(false);

        if (release is null)
        {
            return null;
        }

        if (new Version(release.Version) > currentVersion)
        {
            _update = release;
            _logger.LogInformation($"Found new version {_update.Version}");

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Download latest release from GitHub and create update lock file.
    /// </summary>
    public Task DownloadAndUnpackLatestRelease(CancellationToken cancellationToken = default) =>
        DownloadAndUnpackLatestRelease(ClientProperties.WorkingFolder, ClientProperties.ExecutableName, cancellationToken);

    /// <summary> Prepares a validated update in a fresh directory before publishing its marker. </summary>
    /// <param name="workingFolder">The application installation directory.</param>
    /// <param name="executableName">The expected executable filename.</param>
    /// <param name="cancellationToken">Cancels preparation before publishing the update.</param>
    /// <returns>The preparation task.</returns>
    public async Task DownloadAndUnpackLatestRelease(string workingFolder, string executableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(_update?.DownloadUrl);
        var updateFolder = Path.Combine(workingFolder, ClientConsts.UpdateFolder);
        using var replacement = new DirectoryReplacement(updateFolder, _logger);
        var download = replacement.StagingPath + ".zip";
        try
        {
            var result = await _filesDownloader.DownloadFileAsync(_update.DownloadUrl, download, cancellationToken).ConfigureAwait(false);
            if (result.ResultEnum == Core.All.ResultEnum.Cancelled)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            if (!result.IsSuccess)
            {
                throw new IOException($"Update download failed: {result.Message}");
            }

            if (!await FileHash.MatchesAsync(download, _update.Hash, cancellationToken).ConfigureAwait(false))
            {
                throw new InvalidDataException("The update SHA-256 digest does not match the release.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            await ZipFile.ExtractToDirectoryAsync(download, replacement.StagingPath, cancellationToken).ConfigureAwait(false);
            var executable = Path.Combine(replacement.StagingPath, executableName);
            if (!File.Exists(executable) || new FileInfo(executable).Length == 0)
            {
                throw new InvalidDataException("The update does not contain the expected executable.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            replacement.Commit();
            await File.WriteAllTextAsync(Path.Combine(workingFolder, ClientConsts.UpdateFile), string.Empty, CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            foreach (var path in new[] { download, download + ".temp" })
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not remove update download {Path}.", path);
                }
            }
        }
    }

    /// <summary> Replaces all prepared application files, rolling back a failed installation. </summary>
    /// <param name="workingFolder">The application installation directory.</param>
    /// <param name="executableName">The expected executable filename.</param>
    public static void ApplyPreparedUpdate(string workingFolder, string executableName)
    {
        var updateFolder = Path.Combine(workingFolder, ClientConsts.UpdateFolder);
        var stagedExecutable = Path.Combine(updateFolder, executableName);
        if (!File.Exists(stagedExecutable) || new FileInfo(stagedExecutable).Length == 0)
        {
            throw new InvalidDataException("The prepared update does not contain the expected executable.");
        }

        var files = Directory.GetFiles(updateFolder, "*", SearchOption.AllDirectories);
        var backup = Path.Combine(workingFolder, ".update-backup-" + Guid.NewGuid().ToString("N"));
        var changed = new List<(string Destination, string Backup, bool Existed)>();
        _ = Directory.CreateDirectory(backup);
        try
        {
            foreach (var source in files.Order(StringComparer.Ordinal))
            {
                var relative = Path.GetRelativePath(updateFolder, source);
                var destination = Path.Combine(workingFolder, relative);
                var original = Path.Combine(backup, relative);
                _ = Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                _ = Directory.CreateDirectory(Path.GetDirectoryName(original)!);
                var existed = File.Exists(destination);
                if (existed)
                {
                    File.Move(destination, original);
                }

                changed.Add((destination, original, existed));
                File.Copy(source, destination);
            }

            if (OperatingSystem.IsLinux())
            {
                var executable = Path.Combine(workingFolder, executableName);
                File.SetUnixFileMode(executable, File.GetUnixFileMode(executable) | UnixFileMode.UserExecute);
            }

            File.Delete(Path.Combine(workingFolder, ClientConsts.UpdateFile));
        }
        catch (Exception failure)
        {
            var failures = new List<Exception> { failure };
            foreach (var item in changed.AsEnumerable().Reverse())
            {
                try
                {
                    // A directory collision means no replacement file was created.
                    if (File.Exists(item.Destination))
                    {
                        File.Delete(item.Destination);
                    }

                    if (item.Existed)
                    {
                        File.Move(item.Backup, item.Destination);
                    }
                }
                catch (Exception rollbackFailure)
                {
                    failures.Add(rollbackFailure);
                }
            }

            if (failures.Count > 1)
            {
                throw new AggregateException($"Update rollback was incomplete. Original files remain at {backup}.", failures);
            }

            CompleteUpdateBackup(backup);
            throw;
        }

        CompleteUpdateBackup(backup);
        TryRemoveUpdateDirectory(updateFolder);
    }

    /// <summary> Marks a fully committed or restored backup for safe deferred deletion. </summary>
    /// <param name="backup">The backup whose recovery work is complete.</param>
    private static void CompleteUpdateBackup(string backup)
    {
        try
        {
            File.WriteAllText(backup + ".cleanup-ready", string.Empty);
            RemoveCompletedUpdateBackup(backup);
        }
        catch (Exception ex)
        {
            Trace.TraceWarning("Could not mark completed update backup {0}: {1}", backup, ex);
        }
    }

    /// <summary> Retries cleanup of completed updates while retaining incomplete rollback backups. </summary>
    /// <param name="workingFolder">The application installation directory.</param>
    public static void CleanupCompletedUpdates(string workingFolder)
    {
        try
        {
            foreach (var marker in Directory.EnumerateFiles(workingFolder, ".update-backup-*.cleanup-ready"))
            {
                var backup = marker[..^".cleanup-ready".Length];
                if (!Guid.TryParseExact(Path.GetFileName(backup)[".update-backup-".Length..], "N", out _) ||
                    (Directory.Exists(backup) && (File.GetAttributes(backup) & FileAttributes.ReparsePoint) != 0) ||
                    !File.Exists(backup + ".cleanup-ready"))
                {
                    continue;
                }

                RemoveCompletedUpdateBackup(backup);
            }
        }
        catch (Exception ex)
        {
            Trace.TraceWarning("Could not enumerate completed update backups in {0}: {1}", workingFolder, ex);
        }
    }

    /// <summary> Retains the sidecar marker until all completed backup files have been removed. </summary>
    /// <param name="backup">The completed backup directory.</param>
    private static void RemoveCompletedUpdateBackup(string backup)
    {
        if (Directory.Exists(backup))
        {
            TryRemoveUpdateDirectory(backup);
        }

        if (!Directory.Exists(backup))
        {
            File.Delete(backup + ".cleanup-ready");
        }
    }

    /// <summary> Cleans update artifacts without converting a completed replacement into a failure. </summary>
    /// <param name="path">The operation-owned directory.</param>
    private static void TryRemoveUpdateDirectory(string path)
    {
        try
        {
            Directory.Delete(path, true);
        }
        catch (Exception ex)
        {
            Trace.TraceWarning("Could not remove update directory {0}: {1}", path, ex);
        }
    }

    /// <summary>
    ///     Install update.
    /// </summary>
    public static void InstallUpdate()
    {
        try
        {
            var dir = ClientProperties.WorkingFolder;
            var oldExe = Path.Combine(dir, ClientProperties.ExecutableName);
            ApplyPreparedUpdate(dir, ClientProperties.ExecutableName);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                //starting new version of the app
                using var process = Process.Start(oldExe);
            }

            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            var crashLog = Path.Combine(ClientProperties.WorkingFolder, $"{DateTime.Now:dd_MM_yy_HH_mm}.update_crashlog");
            try
            {
                File.WriteAllText(crashLog, $"=== Critical error during update installation: {ex} ===");
            }
            catch
            {
                // Best-effort crash log write
            }

            Environment.Exit(1);
        }
    }
}
