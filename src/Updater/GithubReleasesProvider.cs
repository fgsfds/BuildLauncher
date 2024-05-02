using Common.Helpers;
using System.Runtime.InteropServices;
using System.Text.Json;
using Updater.Entities;

namespace Updater
{
    public static class GitHubReleasesProvider
    {
        /// <summary>
        /// Return the latest new release or null if there's no newer releases
        /// </summary>
        /// <param name="currentVersion">current release version</param>
        public static async Task<AppUpdateEntity?> GetLatestUpdateAsync(Version currentVersion)
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("BuildLauncher");

            var json = await client.GetStringAsync(Consts.GitHubReleases).ConfigureAwait(false);

            var releases = JsonSerializer.Deserialize(json, GitHubReleaseContext.Default.ListGitHubRelease)
                ?? ThrowHelper.Exception<List<GitHubRelease>>("Error while deserializing GitHub releases");

            if (!CommonProperties.IsDevMode)
            {
                releases = [.. releases.Where(static x => x.IsDraft is false && x.IsPrerelease is false)];
            }

            string osPostfix = string.Empty;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                osPostfix = "win-x64.zip";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                osPostfix = "linux-x64.zip";
            }
            else
            {
                ThrowHelper.PlatformNotSupportedException();
            }

            AppUpdateEntity? update = null;

            foreach (var release in releases)
            {
                var asset = release.Assets.FirstOrDefault(x => x.FileName.EndsWith(osPostfix));

                if (asset is null)
                {
                    continue;
                }

                var version = new Version(release.TagName);

                if (version <= currentVersion ||
                    version < update?.Version)
                {
                    continue;
                }

                var description = release.Description;
                var downloadUrl = new Uri(asset.DownloadUrl);

                update = new()
                {
                    Version = version,
                    Description = description,
                    DownloadUrl = downloadUrl
                };
            }

            return update;
        }
    }
}
