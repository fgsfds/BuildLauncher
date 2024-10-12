using Common.Entities;
using Common.Helpers;
using Common.Server.Entities;
using CommunityToolkit.Diagnostics;
using System.Text.Json;

namespace Web.Blazor.Providers;

public sealed class AppReleasesProvider
{
    private readonly ILogger<AppReleasesProvider> _logger;
    private readonly HttpClient _httpClient;

    public GeneralReleaseEntity? AppRelease { get; private set; }

    public AppReleasesProvider(
        ILogger<AppReleasesProvider> logger,
        HttpClient httpClient
        )
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Return the latest new release or null if there's no newer releases
    /// </summary>
    public async Task GetLatestVersionAsync()
    {
        try
        {
            _logger.LogInformation("Looking for new app release");

            using var response = await _httpClient.GetAsync(Consts.GitHubReleases, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error while getting releases" + Environment.NewLine + response.StatusCode);
                return;
            }

            var releasesJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var releases =
                JsonSerializer.Deserialize(releasesJson, GitHubReleaseContext.Default.ListGitHubReleaseEntity)
                ?? ThrowHelper.ThrowFormatException<List<GitHubReleaseEntity>>("Error while deserializing GitHub releases");

            releases = [.. releases.Where(static x => x.IsDraft is false && x.IsPrerelease is false).OrderByDescending(static x => new Version(x.TagName))];

            var appRelease = GetAppUpdateEntity(releases[0]);

            AppRelease = appRelease;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while getting latest app release");
            _logger.LogError(ex.ToString());
        }
    }

    private GeneralReleaseEntity GetAppUpdateEntity(GitHubReleaseEntity release)
    {
        var windowsAsset = release.Assets.FirstOrDefault(x => x.FileName.EndsWith("win-x64.zip"));
        var linuxAsset = release.Assets.FirstOrDefault(x => x.FileName.EndsWith("linux-x64.zip"));

        GeneralReleaseEntity update = new()
        {
            Version = release.TagName,
            Description = release.Description,
            WindowsDownloadUrl = windowsAsset is null ? null : new Uri(windowsAsset.DownloadUrl),
            LinuxDownloadUrl = linuxAsset is null ? null : new Uri(linuxAsset.DownloadUrl)
        };

        return update;
    }
}
