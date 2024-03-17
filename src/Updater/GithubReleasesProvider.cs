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
                releases = [.. releases.Where(static x => x.draft is false && x.prerelease is false)];
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
                var asset = release.assets.FirstOrDefault(x => x.name.EndsWith(osPostfix));

                if (asset is null)
                {
                    continue;
                }

                var version = new Version(release.tag_name);

                if (version <= currentVersion ||
                    version < update?.Version)
                {
                    continue;
                }

                var description = release.body;
                var downloadUrl = new Uri(asset.browser_download_url);

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
