using Common.Entities;
using Common.Enums;
using Common.Helpers;
using Common.Releases;
using Ports.Ports;
using Ports.Providers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Superheater.Web.Server.Providers
{
    public sealed partial class PortsReleasesProvider
    {
        private readonly ILogger<PortsReleasesProvider> _logger;
        private readonly PortsProvider _portsProvider;
        private readonly HttpClient _httpClient;

        public Dictionary<PortEnum, GeneralReleaseEntity> PortsReleases { get; set; }

        public PortsReleasesProvider(
            ILogger<PortsReleasesProvider> logger,
            PortsProvider portsProvider,
            HttpClient httpClient)
        {
            _portsProvider = portsProvider;
            _logger = logger;
            _httpClient = httpClient;

            PortsReleases = new();
        }

        public async Task GetLatestReleasesAsync()
        {
            var ports = _portsProvider.GetAllPorts();

            foreach (var port in ports)
            {
                await GetLatestReleaseAsync(port).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Get the latest release of the selected port
        /// </summary>
        /// <param name="port">Port</param>
        private async Task GetLatestReleaseAsync(BasePort port)
        {
            if (port.RepoUrl is null)
            {
                return;
            }

            if (port.PortEnum is PortEnum.VoidSW)
            {
                return;
            }

            if (port.PortEnum is PortEnum.BuildGDX)
            {
                GeneralReleaseEntity bgdxRelease = new()
                {
                    Description = string.Empty,
                    Version = "1.16",
                    WindowsDownloadUrl = port.RepoUrl,
                    LinuxDownloadUrl = null
                };

                PortsReleases.Add(PortEnum.BuildGDX, bgdxRelease);
                return;
            }

            var response = await _httpClient.GetStringAsync(port.RepoUrl).ConfigureAwait(false);

            if (port.PortEnum is PortEnum.EDuke32)
            {
                EDuke32Hack(response);
                return;
            }

            var releases = JsonSerializer.Deserialize(response, GitHubReleaseContext.Default.ListGitHubReleaseEntity)
                ?? ThrowHelper.Exception<List<GitHubReleaseEntity>>("Error while deserializing GitHub releases");

            var release = releases.FirstOrDefault(static x => x.IsDraft is false && x.IsPrerelease is false);

            if (release is null)
            {
                return;
            }

            var zip = release.Assets.FirstOrDefault(port.WindowsReleasePredicate);

            if (zip is null)
            {
                return;
            }

            GeneralReleaseEntity portRelease = new()
            {
                Description = release.Description,
                Version = GetVersion(port, release, zip),
                WindowsDownloadUrl = new(zip.DownloadUrl),
                LinuxDownloadUrl = null
            };

            PortsReleases.Add(port.PortEnum, portRelease);
        }

        /// <summary>
        /// Get port version
        /// </summary>
        private string GetVersion(BasePort port, GitHubReleaseEntity release, GitHubReleaseAsset zip)
        {
            string version;

            if (port.PortEnum is PortEnum.NotBlood)
            {
                version = zip.UpdatedDate.ToString("dd.MM.yyyy");
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
        private void EDuke32Hack(string response)
        {
            var regex = EDuke32WindowsReleaseRegex();
            var fileName = regex.Matches(response).FirstOrDefault();

            if (fileName is null)
            {
                return;
            }

            var regexVersion = EDuke32VersionRegex();
            var version = regexVersion.Matches(fileName.ToString()).FirstOrDefault();

            if (version is null)
            {
                return;
            }

            GeneralReleaseEntity release = new()
            {
                Description = string.Empty,
                Version = "r" + version.ToString(),
                WindowsDownloadUrl = new($"https://dukeworld.com/eduke32/synthesis/latest/{fileName}"),
                LinuxDownloadUrl = null
            };

            PortsReleases.Add(PortEnum.EDuke32, release);
        }

        [GeneratedRegex("eduke32_win64_2[^\"]*")]
        private partial Regex EDuke32WindowsReleaseRegex();

        [GeneratedRegex(@"(?<=\-)(\d*)(?=\-)")]
        private partial Regex EDuke32VersionRegex();
    }
}
