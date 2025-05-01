using Common.Entities;
using Common.Enums;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Tools.Providers;

internal sealed class ToolsReleasesRepoRetriever
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly ToolsRepositoriesProvider _repoProvider;

    public Dictionary<ToolEnum, GeneralReleaseEntity> ToolsReleases { get; set; }

    public ToolsReleasesRepoRetriever(
        ILogger logger,
        HttpClient httpClient,
        ToolsRepositoriesProvider repoProvider
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
                _logger.LogCritical(ex.ToString());
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

        var releases = JsonSerializer.Deserialize(response, GitHubReleaseEntityContext.Default.ListGitHubReleaseEntity)
            ?? ThrowHelper.ThrowFormatException<List<GitHubReleaseEntity>>("Error while deserializing GitHub releases");

        var release = releases.FirstOrDefault(static x => !x.IsDraft && !x.IsPrerelease);

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
            SupportedOS = OSEnum.Windows,
            Description = release.Description,
            Version = version.ToString("dd.MM.yyyy"),
            DownloadUrl = new(zip.DownloadUrl)
        };

        return toolRelease;
    }
}
