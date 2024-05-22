using ClientCommon.API;
using ClientCommon.Helpers;
using ClientCommon.Providers;
using Common.Entities;
using Common.Helpers;
using Common.Tools;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace ClientCommon
{
    public sealed class AppUpdateInstaller
    {
        private readonly ArchiveTools _archiveTools;
        private readonly ApiInterface _apiInterface;

        private GeneralReleaseEntity? _update;

        public AppUpdateInstaller(
            ArchiveTools archiveTools,
            ApiInterface apiInterface
            )
        {
            _archiveTools = archiveTools;
            _apiInterface = apiInterface;
        }

        /// <summary>
        /// Check GitHub for releases with version higher than current
        /// </summary>
        /// <param name="currentVersion">Current SFD version</param>
        /// <returns></returns>
        public async Task<bool> CheckForUpdates(Version currentVersion)
        {
            var release = await _apiInterface.GetLatestAppReleaseAsync().ConfigureAwait(false);

            if (release is not null &&
                new Version(release.Version) > currentVersion)
            {
                _update = release;
                return true;

                //Logger.Info($"Found new version {_update!.Version}");
            }

            return false;
        }

        /// <summary>
        /// Download latest release from Github and create update lock file
        /// </summary>
        /// <returns></returns>
        public async Task DownloadAndUnpackLatestRelease()
        {
            _update.ThrowIfNull();
            _update.WindowsDownloadUrl.ThrowIfNull();

            var updateUrl = _update.WindowsDownloadUrl;

            var fileName = Path.Combine(ClientProperties.ExeFolderPath, Path.GetFileName(updateUrl.ToString()).Trim());

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            await _archiveTools.DownloadFileAsync(updateUrl, fileName).ConfigureAwait(false);

            ZipFile.ExtractToDirectory(fileName, Path.Combine(ClientProperties.ExeFolderPath, Consts.UpdateFolder), true);

            File.Delete(fileName);

            await File.Create(Consts.UpdateFile).DisposeAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Install update
        /// </summary>
        public static void InstallUpdate()
        {
            var dir = ClientProperties.ExeFolderPath;
            var updateDir = Path.Combine(dir, Consts.UpdateFolder);
            var oldExe = Path.Combine(dir, ClientProperties.ExecutableName);
            var newExe = Path.Combine(updateDir, ClientProperties.ExecutableName);

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
