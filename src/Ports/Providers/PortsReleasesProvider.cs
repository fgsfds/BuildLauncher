using Common.Enums;
using Common.Helpers;
using Ports.Ports;
using Ports.Ports.EDuke32;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

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
        public static async Task<PortRelease?> GetLatestReleaseAsync(BasePort port)
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

                response = await client.GetStringAsync(port.RepoUrl);
            }
            catch (Exception)
            {
                return null;
            }

            var a = port.GetType();

            if (port.PortEnum == PortEnum.EDuke32)
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

            int version = 0;

            if (port is NotBlood)
            {
                version = zip.UpdatedDate.Day * 1_00_0000 + zip.UpdatedDate.Month * 1_0000 + zip.UpdatedDate.Year;
            }
            else
            {
                var numbersOnly = new string(release.TagName.Where(static x => char.IsDigit(x)).ToArray());
                version = int.Parse(numbersOnly);
            }

            return new(zip.DownloadUrl, version);
        }

        /// <summary>
        /// Hack to get EDuke32 release since dukeworld doesn't have API
        /// </summary>
        /// <param name="response">Json response</param>
        private static PortRelease? EDuke32Hack(string response)
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

            return new($"https://dukeworld.com/eduke32/synthesis/latest/{fileName}", int.Parse(version.ToString()));
        }

        [GeneratedRegex("eduke32_win64_2[^\"]*")]
        private static partial Regex EDuke32WindowsReleaseRegex();

        [GeneratedRegex(@"(?<=\-)(\d*)(?=\-)")]
        private static partial Regex EDuke32VersionRegex();
    }


    public sealed class PortRelease
    {
        public readonly string Url;
        public readonly int Version;

        public PortRelease(string url, int version)
        {
            Url = url;
            Version = version;
        }
    }

#pragma warning disable IDE1006 // Naming Styles
    public sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; }

        [JsonPropertyName("draft")]
        public bool IsDraft { get; set; }

        [JsonPropertyName("prerelease")]
        public bool IsPrerelease { get; set; }

        [JsonPropertyName("assets")]
        public GitHubReleaseAsset[] Assets { get; set; }

        [JsonPropertyName("body")]
        public string Description { get; set; }
    }

    public sealed class GitHubReleaseAsset
    {
        [JsonPropertyName("name")]
        public string FileName { get; set; }

        [JsonPropertyName("browser_download_url")]
        public string DownloadUrl { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedDate { get; set; }
    }
#pragma warning restore IDE1006 // Naming Styles


    [JsonSerializable(typeof(List<GitHubRelease>))]
    internal sealed partial class GitHubReleaseContext : JsonSerializerContext { }
}
