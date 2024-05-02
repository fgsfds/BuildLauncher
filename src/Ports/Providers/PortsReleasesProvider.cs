using Common.Enums;
using Common.Helpers;
using Ports.Ports;
using Ports.Ports.EDuke32;
using System.Text.Json;
using System.Text.RegularExpressions;
using Tools.Tools;

namespace Ports.Providers
{
    /// <summary>
    /// Class that provides releases from ports' repositories
    /// </summary>
    public static partial class PortsReleasesProvider
    {
        /// <summary>
        /// Get the latest release of the selected port
        /// </summary>
        /// <param name="port">Port</param>
        public static async Task<CommonRelease?> GetLatestReleaseAsync(BaseTool port)
        {
            if (CommonProperties.IsDevMode)
            {
                return null;
            }

            string response;

            try
            {
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromMinutes(1);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("BuildLauncher");

                response = await client.GetStringAsync(port.RepoUrl).ConfigureAwait(false);
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

            var zip = release.Assets.FirstOrDefault(port.WindowsReleasePredicate);

            if (zip is null)
            {
                return null;
            }

            var version = release.TagName;

            return new(zip.DownloadUrl, version);
        }
        /// <summary>
        /// Get the latest release of the selected port
        /// </summary>
        /// <param name="port">Port</param>
        public static async Task<CommonRelease?> GetLatestReleaseAsync(BasePort port)
        {
            if (port.PortEnum is PortEnum.BuildGDX)
            {
                return new(port.RepoUrl.ToString(), "1.16");
            }

            if (CommonProperties.IsDevMode)
            {
                return null;
            }

            string response;

            try
            {
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromMinutes(1);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("BuildLauncher");

                response = await client.GetStringAsync(port.RepoUrl).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }

            if (port.PortEnum is PortEnum.EDuke32)
            {
                return EDuke32Hack(response);
            }

            var releases = JsonSerializer.Deserialize(response, GitHubReleaseContext.Default.ListGitHubRelease)
                ?? ThrowHelper.Exception<List<GitHubRelease>>("Error while deserializing GitHub releases");

            var release = releases.FirstOrDefault(static x => x.IsDraft is false && x.IsPrerelease is false);

            if (release is null)
            {
                return null;
            }

            var zip = release.Assets.FirstOrDefault(port.WindowsReleasePredicate);

            if (zip is null)
            {
                return null;
            }

            var version = GetVersion(port, release, zip);

            return new(zip.DownloadUrl, version);
        }

        /// <summary>
        /// Get port version
        /// </summary>
        private static string GetVersion(BasePort port, GitHubRelease? release, GitHubReleaseAsset? zip)
        {
            string version;

            if (port is NotBlood)
            {
                version = (zip.UpdatedDate.Day.ToString("00") + "." + zip.UpdatedDate.Month.ToString("00") + "." + zip.UpdatedDate.Year).ToString();
            }
            else
            {
                version = release.TagName;
            }

            return version;
        }

        /// <summary>
        /// Hack to get EDuke32 release since dukeworld doesn't have API
        /// </summary>
        /// <param name="response">Json response</param>
        private static CommonRelease? EDuke32Hack(string response)
        {
            var regex = EDuke32WindowsReleaseRegex();
            var fileName = regex.Matches(response).FirstOrDefault();

            if (fileName is null)
            {
                return null;
            }

            var regexVersion = EDuke32VersionRegex();
            var version = regexVersion.Matches(fileName.ToString()).FirstOrDefault();

            if (version is null)
            {
                return null;
            }

            return new($"https://dukeworld.com/eduke32/synthesis/latest/{fileName}", "r" + version.ToString());
        }

        [GeneratedRegex("eduke32_win64_2[^\"]*")]
        private static partial Regex EDuke32WindowsReleaseRegex();

        [GeneratedRegex(@"(?<=\-)(\d*)(?=\-)")]
        private static partial Regex EDuke32VersionRegex();
    }
}
