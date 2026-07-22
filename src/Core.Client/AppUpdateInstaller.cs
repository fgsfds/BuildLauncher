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
    /// <param name="currentVersion">Current SFD version</param>
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
    public async Task DownloadAndUnpackLatestRelease(CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(_update?.DownloadUrl);

        var updateUrl = _update.DownloadUrl;

        var fileName = Path.Combine(ClientProperties.WorkingFolder, Path.GetFileName(updateUrl.ToString()).Trim());

        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }

        _ = await _filesDownloader.DownloadFileAsync(updateUrl, fileName, cancellationToken).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        ZipFile.ExtractToDirectory(fileName, Path.Combine(ClientProperties.WorkingFolder, ClientConsts.UpdateFolder), true);

        File.Delete(fileName);

        await File.Create(ClientConsts.UpdateFile).DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     Install update.
    /// </summary>
    public static void InstallUpdate()
    {
        try
        {
            var dir = ClientProperties.WorkingFolder;
            var updateDir = Path.Combine(dir, ClientConsts.UpdateFolder);
            var oldExe = Path.Combine(dir, ClientProperties.ExecutableName);
            var newExe = Path.Combine(updateDir, ClientProperties.ExecutableName);

            //renaming old file
            File.Move(oldExe, oldExe + ".old", true);

            //moving new file
            File.Move(newExe, oldExe, true);

            File.Delete(Path.Combine(dir, ClientConsts.UpdateFile));
            Directory.Delete(Path.Combine(dir, ClientConsts.UpdateFolder), true);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                #pragma warning disable IDISP004 // Don't ignore created IDisposable
                //starting new version of the app
                _ = Process.Start(oldExe);
                #pragma warning restore IDISP004 // Don't ignore created IDisposable
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                //setting execute permission for user, otherwise the app won't run from game mode
                var attributes = File.GetUnixFileMode(oldExe);
                File.SetUnixFileMode(oldExe, attributes | UnixFileMode.UserExecute);
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
