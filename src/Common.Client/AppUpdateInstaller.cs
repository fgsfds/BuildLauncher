﻿using System.IO.Compression;
using System.Runtime.InteropServices;
using Common.Client.Helpers;
using Common.Client.Interfaces;
using Common.Client.Tools;
using Common.Common.Serializable.Downloadable;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Common.Client;

public sealed class AppUpdateInstaller
{
    private readonly ArchiveTools _archiveTools;
    private readonly IApiInterface _apiInterface;
    private readonly ILogger _logger;

    private GeneralReleaseJsonModel? _update;

    public AppUpdateInstaller(
        ArchiveTools archiveTools,
        IApiInterface apiInterface,
        ILogger logger
        )
    {
        _archiveTools = archiveTools;
        _apiInterface = apiInterface;
        _logger = logger;
    }

    /// <summary>
    /// Check GitHub for releases with version higher than current
    /// </summary>
    /// <param name="currentVersion">Current SFD version</param>
    public async Task<bool?> CheckForUpdates(Version currentVersion)
    {
        var release = await _apiInterface.GetLatestAppReleaseAsync().ConfigureAwait(false);

        if (release is null)
        {
            return null;
        }

        if (release is not null &&
            new Version(release.Version) > currentVersion)
        {
            _update = release;
            _logger.LogInformation($"Found new version {_update.Version}");

            return true;
        }

        return false;
    }

    /// <summary>
    /// Download latest release from GitHub and create update lock file
    /// </summary>
    public async Task DownloadAndUnpackLatestRelease()
    {
        Guard.IsNotNull(_update?.DownloadUrl);

        var updateUrl = _update.DownloadUrl;

        var fileName = Path.Combine(ClientProperties.WorkingFolder, Path.GetFileName(updateUrl.ToString()).Trim());

        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }

        _ = await _archiveTools.DownloadFileAsync(updateUrl, fileName, CancellationToken.None).ConfigureAwait(false);

        ZipFile.ExtractToDirectory(fileName, Path.Combine(ClientProperties.WorkingFolder, ClientConsts.UpdateFolder), true);

        File.Delete(fileName);

        await File.Create(ClientConsts.UpdateFile).DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Install update
    /// </summary>
    public static void InstallUpdate()
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
            //starting new version of the app
            _ = System.Diagnostics.Process.Start(oldExe);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            //setting execute permission for user, otherwise the app won't run from game mode
            var attributes = File.GetUnixFileMode(oldExe);
            File.SetUnixFileMode(oldExe, attributes | UnixFileMode.UserExecute);
        }

        Environment.Exit(0);
    }
}
