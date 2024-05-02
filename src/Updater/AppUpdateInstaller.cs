using Common.Helpers;
using Common.Tools;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Updater
{
    public sealed class AppUpdateInstaller(ArchiveTools archiveTools)
    {
        private readonly ArchiveTools _archiveTools = archiveTools;

        private AppRelease? _update;

        /// <summary>
        /// Check GitHub for releases with version higher than current
        /// </summary>
        /// <param name="currentVersion">Current SFD version</param>
        /// <returns></returns>
        public async Task<bool> CheckForUpdates(Version currentVersion)
        {
            _update = await AppReleasesProvider.GetLatestUpdateAsync(currentVersion).ConfigureAwait(false);

            var hasUpdate = _update is not null;

            if (hasUpdate)
            {
                //Logger.Info($"Found new version {_update!.Version}");
            }

            return hasUpdate;
        }

        /// <summary>
        /// Download latest release from Github and create update lock file
        /// </summary>
        /// <returns></returns>
        public async Task DownloadAndUnpackLatestRelease()
        {
            _update.ThrowIfNull();

            Uri updateUrl = new(_update.Url);

            var fileName = Path.Combine(CommonProperties.ExeFolderPath, Path.GetFileName(updateUrl.ToString()).Trim());

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            await _archiveTools.DownloadFileAsync(updateUrl, fileName).ConfigureAwait(false);

            ZipFile.ExtractToDirectory(fileName, Path.Combine(CommonProperties.ExeFolderPath, Consts.UpdateFolder), true);

            File.Delete(fileName);

            await File.Create(Consts.UpdateFile).DisposeAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Install update
        /// </summary>
        public static void InstallUpdate()
        {
            var dir = CommonProperties.ExeFolderPath;
            var updateDir = Path.Combine(dir, Consts.UpdateFolder);
            var oldExe = Path.Combine(dir, CommonProperties.ExecutableName);
            var newExe = Path.Combine(updateDir, CommonProperties.ExecutableName);

            //renaming old file
            File.Move(oldExe, oldExe + ".old", true);

            //moving new file
            File.Move(newExe, oldExe, true);

            File.Delete(Path.Combine(dir, Consts.UpdateFile));
            Directory.Delete(Path.Combine(dir, Consts.UpdateFolder), true);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                //starting new version of the app
                System.Diagnostics.Process.Start(oldExe);
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
}
