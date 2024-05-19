using Common.Entities;
using Common.Enums;
using Common.Helpers;
using Common.Providers;
using Common.Releases;
using System.Text.Json;

namespace Superheater.Web.Server.Providers
{
    public sealed class ToolsReleasesProvider
    {
        private readonly ILogger<ToolsReleasesProvider> _logger;
        private readonly HttpClient _httpClient;
        private readonly RepositoriesProvider _repoProvider;

        public Dictionary<ToolEnum, GeneralReleaseEntity> ToolsReleases { get; set; }

        public ToolsReleasesProvider(
            ILogger<ToolsReleasesProvider> logger,
            HttpClient httpClient,
            RepositoriesProvider repoProvider
            )
        {
            _logger = logger;
            _httpClient = httpClient;
            _repoProvider = repoProvider;

            ToolsReleases = new();
        }

        public async Task GetLatestReleasesAsync()
        {
            var tools = Enum.GetValues<ToolEnum>();

            foreach (var tool in tools)
            {
                await GetLatestReleaseAsync(tool).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Get the latest release of the selected port
        /// </summary>
        /// <param name="port">Port</param>
        private async Task GetLatestReleaseAsync(ToolEnum toolEnum)
        {
            var repo = _repoProvider.GetToolRepo(toolEnum);

            if (repo.RepoUrl is null || repo.WindowsReleasePredicate is null)
            {
                return;
            }

            var response = await _httpClient.GetStringAsync(repo.RepoUrl).ConfigureAwait(false);

            var releases = JsonSerializer.Deserialize(response, GitHubReleaseContext.Default.ListGitHubReleaseEntity)
                ?? ThrowHelper.Exception<List<GitHubReleaseEntity>>("Error while deserializing GitHub releases");

            var release = releases.FirstOrDefault(static x => x.IsDraft is false && x.IsPrerelease is false);

            if (release is null)
            {
                return;
            }

            var zip = release.Assets.FirstOrDefault(repo.WindowsReleasePredicate);

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

            ToolsReleases.Add(toolEnum, portRelease);
        }
    }
}
