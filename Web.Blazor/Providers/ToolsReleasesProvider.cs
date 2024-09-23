using Common.Entities;
using Common.Enums;
using Common.Helpers;
using Common.Providers;
using Common.Releases;
using System.Text.Json;

namespace Web.Blazor.Providers;

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

        ToolsReleases = [];
    }

    public async Task GetLatestReleasesAsync()
    {
        _logger.LogInformation("Looking for new tools releases");

        ToolsReleases.Clear();

        var tools = Enum.GetValues<ToolEnum>();

        foreach (var tool in tools)
        {
            try
            {
                var newRelease = await GetLatestReleaseAsync(tool).ConfigureAwait(false);

                if (newRelease is not null)
                {
                    var doesExist = ToolsReleases.TryGetValue(tool, out _);

                    if (doesExist)
                    {
                        ToolsReleases[tool] = newRelease;
                    }
                    else
                    {
                        ToolsReleases.Add(tool, newRelease);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while getting latest release for {tool}");
                _logger.LogError(ex.ToString());
            }
        }
    }

    /// <summary>
    /// Get the latest release of the selected port
    /// </summary>
    /// <param name="toolEnum">Tool enum</param>
    private async Task<GeneralReleaseEntity?> GetLatestReleaseAsync(ToolEnum toolEnum)
    {
        var repo = _repoProvider.GetToolRepo(toolEnum);

        if (repo.RepoUrl is null || repo.WindowsReleasePredicate is null)
        {
            return null;
        }

        var response = await _httpClient.GetStringAsync(repo.RepoUrl).ConfigureAwait(false);

        var releases = JsonSerializer.Deserialize(response, GitHubReleaseContext.Default.ListGitHubReleaseEntity)
            ?? ThrowHelper.Exception<List<GitHubReleaseEntity>>("Error while deserializing GitHub releases");

        var release = releases.FirstOrDefault(static x => x.IsDraft is false && x.IsPrerelease is false);

        if (release is null)
        {
            return null;
        }

        var zip = release.Assets.FirstOrDefault(repo.WindowsReleasePredicate);

        if (zip is null)
        {
            return null;
        }

        var version = zip.UpdatedDate;

        GeneralReleaseEntity toolRelease = new()
        {
            Description = release.Description,
            Version = version.ToString("dd.MM.yyyy"),
            WindowsDownloadUrl = new(zip.DownloadUrl),
            LinuxDownloadUrl = null
        };

        return toolRelease;
    }
}
