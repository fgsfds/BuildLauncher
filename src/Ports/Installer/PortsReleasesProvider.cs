using Common.Config;
using Common.Enums;
using Common.Helpers;
using Common.Releases;
using Common.Tools;
using Ports.Ports;
using Ports.Ports.EDuke32;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Ports.Installer
{
    /// <summary>
    /// Class that provides releases from ports' repositories
    /// </summary>
    public partial class PortsReleasesProvider
    {
        private readonly HttpClientInstance _httpClient;
        private readonly ConfigEntity _config;

        public PortsReleasesProvider(
            HttpClientInstance httpClient,
            ConfigProvider config
            )
        {
            _httpClient = httpClient;
            _config = config.Config;
        }

        /// <summary>
        /// Get the latest release of the selected port
        /// </summary>
        /// <param name="port">Port</param>
        public async Task<CommonRelease?> GetLatestReleaseAsync(BasePort port, bool forceCheck)
        {
            if (!forceCheck && (_config.LastUpdateChecks?.TryGetValue(port.Name, out var value) ?? false))
            {
                var timeDifference = (DateTime.Now - DateTime.Parse(value[1])).TotalDays;

                if (timeDifference < 7)
                {
                    return new(value[2], value[0]);
                }
            }


            if (port.PortEnum is PortEnum.BuildGDX)
            {
                return new(port.RepoUrl.ToString(), "1.16");
            }

            if (CommonProperties.IsDevMode)
            {
                return null;
            }

            if (port.RepoUrl is null)
            {
                return null;
            }

            string response;

            try
            {
                response = await _httpClient.GetStringAsync(port.RepoUrl).ConfigureAwait(false);
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

            _config.AddLastUpdateCheck(port.Name, version, DateTime.Now, zip.DownloadUrl);

            return new(zip.DownloadUrl, version);
        }

        /// <summary>
        /// Get port version
        /// </summary>
        private string GetVersion(BasePort port, GitHubRelease release, GitHubReleaseAsset zip)
        {
            string version;

            if (port is NotBlood)
            {
                version = zip.UpdatedDate.Day.ToString("00") + "." + zip.UpdatedDate.Month.ToString("00") + "." + zip.UpdatedDate.Year;
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
        private CommonRelease? EDuke32Hack(string response)
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


            _config.AddLastUpdateCheck("EDuke32", "r" + version.ToString(), DateTime.Now, $"https://dukeworld.com/eduke32/synthesis/latest/{fileName}");
            
            return new($"https://dukeworld.com/eduke32/synthesis/latest/{fileName}", "r" + version.ToString());
        }

        [GeneratedRegex("eduke32_win64_2[^\"]*")]
        private partial Regex EDuke32WindowsReleaseRegex();

        [GeneratedRegex(@"(?<=\-)(\d*)(?=\-)")]
        private partial Regex EDuke32VersionRegex();
    }
}
