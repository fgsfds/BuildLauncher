using Common.Entities;
using Common.Enums;
using Common.Helpers;
using Common.Releases;
using System.Text.Json;
using Tools.Providers;
using Tools.Tools;

namespace Superheater.Web.Server.Providers
{
    public sealed class ToolsReleasesProvider
    {
        private readonly ILogger<ToolsReleasesProvider> _logger;
        private readonly HttpClient _httpClient;
        private readonly ToolsProvider _toolsProvider;

        public Dictionary<ToolEnum, GeneralReleaseEntity> ToolsReleases { get; set; }

        public ToolsReleasesProvider(
            ILogger<ToolsReleasesProvider> logger,
            HttpClient httpClient,
            ToolsProvider toolsProvider
            )
        {
            _logger = logger;
            _httpClient = httpClient;
            _toolsProvider = toolsProvider;

            ToolsReleases = new();
        }

        public async Task GetLatestReleasesAsync()
        {
            var tools = _toolsProvider.GetAllTools();

            foreach (var tool in tools)
            {
                await GetLatestReleaseAsync(tool).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Get the latest release of the selected port
        /// </summary>
        /// <param name="port">Port</param>
        private async Task GetLatestReleaseAsync(BaseTool tool)
        {
            if (tool.RepoUrl is null)
            {
                return;
            }

            var response = await _httpClient.GetStringAsync(tool.RepoUrl).ConfigureAwait(false);

            var releases = JsonSerializer.Deserialize(response, GitHubReleaseContext.Default.ListGitHubReleaseEntity)
                ?? ThrowHelper.Exception<List<GitHubReleaseEntity>>("Error while deserializing GitHub releases");

            var release = releases.FirstOrDefault(static x => x.IsDraft is false && x.IsPrerelease is false);

            if (release is null)
            {
                return;
            }

            var zip = release.Assets.FirstOrDefault(tool.WindowsReleasePredicate);

            if (zip is null)
            {
                return;
            }

            var version = zip.UpdatedDate;

            GeneralReleaseEntity portRelease = new()
            {
                Description = release.Description,
                Version = version.ToString("dd.MM.yyyy"),
                WindowsDownloadUrl = new(zip.DownloadUrl),
                LinuxDownloadUrl = null
            };

            ToolsReleases.Add(tool.ToolEnum, portRelease);
        }
    }
}
