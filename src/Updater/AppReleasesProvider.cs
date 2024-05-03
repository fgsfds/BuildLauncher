using Common.Helpers;
using Common.Releases;
using Common.Tools;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Updater
{
    public class AppReleasesProvider
    {
        private readonly HttpClientInstance _httpClient;

        public AppReleasesProvider(HttpClientInstance httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Return the latest new release or null if there's no newer releases
        /// </summary>
        /// <param name="currentVersion">current release version</param>
        public async Task<AppRelease?> GetLatestUpdateAsync(Version currentVersion)
        {
            var json = await _httpClient.GetStringAsync(Consts.GitHubReleases).ConfigureAwait(false);

            var releases = JsonSerializer.Deserialize(json, GitHubReleaseContext.Default.ListGitHubRelease)
                ?? ThrowHelper.Exception<List<GitHubRelease>>("Error while deserializing GitHub releases");

            if (!CommonProperties.IsDevMode)
            {
                releases = [.. releases.Where(static x => x.IsDraft is false && x.IsPrerelease is false)];
            }

            var osPostfix = string.Empty;

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

            AppRelease? update = null;

            foreach (var release in releases)
            {
                var asset = release.Assets.FirstOrDefault(x => x.FileName.EndsWith(osPostfix));

                if (asset is null)
                {
                    continue;
                }

                Version version = new(release.TagName);

                if (version <= currentVersion ||
                    version < update?.Version)
                {
                    continue;
                }


                update = new(asset.DownloadUrl, version);
            }

            return update;
        }
    }
}
