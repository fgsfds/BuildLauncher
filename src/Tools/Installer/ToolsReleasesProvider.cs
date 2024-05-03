using Common.Helpers;
using Common.Releases;
using Common.Tools;
using System.Text.Json;
using Tools.Tools;

namespace Tools.Installer
{
    /// <summary>
    /// Class that provides releases from tools' repositories
    /// </summary>
    public partial class ToolsReleasesProvider
    {
        private readonly HttpClientInstance _httpClient;

        public ToolsReleasesProvider(HttpClientInstance httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Get the latest release of the selected tool
        /// </summary>
        /// <param name="tool">Tool</param>
        public async Task<CommonRelease?> GetLatestReleaseAsync(BaseTool tool)
        {
            if (CommonProperties.IsDevMode)
            {
                return null;
            }

            if (tool.RepoUrl is null)
            {
                return null;
            }

            string response;

            try
            {
                response = await _httpClient.GetStringAsync(tool.RepoUrl).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }

            var releases = JsonSerializer.Deserialize(response, GitHubReleaseContext.Default.ListGitHubRelease)
                ?? ThrowHelper.Exception<List<GitHubRelease>>("Error while deserializing GitHub releases");

            var release = releases.FirstOrDefault(static x => x.IsDraft is false && x.IsPrerelease is false);

            if (release is null)
            {
                return null;
            }

            var zip = release.Assets.FirstOrDefault(tool.WindowsReleasePredicate);

            if (zip is null)
            {
                return null;
            }

            var version = zip.UpdatedDate;

            return new(zip.DownloadUrl, version.ToString("dd.MM.yyyy"));
        }
    }
}
