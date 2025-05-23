using System.Text.Json;
using Common.Common.Serializable.Downloadable;
using Common.Enums;
using Common.Helpers;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Common.Common.Providers;

public sealed class RepoAppReleasesRetriever
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;

    public Dictionary<OSEnum, GeneralReleaseJsonModel> AppRelease { get; } = [];

    public RepoAppReleasesRetriever(
        ILogger logger,
        HttpClient httpClient
        )
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Return the latest new release or null if there's no newer releases
    /// </summary>
    public async Task GetLatestVersionAsync(bool includePreReleases)
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
                JsonSerializer.Deserialize(releasesJson, GitHubReleaseEntityContext.Default.ListGitHubReleaseJsonModel)
                ?? ThrowHelper.ThrowFormatException<List<GitHubReleaseJsonModel>>("Error while deserializing GitHub releases");

            if (includePreReleases)
            {
                releases = [.. releases.OrderByDescending(static x => new Version(x.TagName))];
            }
            else
            {
                releases = [.. releases.Where(static x => !x.IsDraft && !x.IsPrerelease).OrderByDescending(static x => new Version(x.TagName))];
            }

            var release = releases[0];

            var windowsAsset = release.Assets.FirstOrDefault(x => x.FileName.EndsWith("win-x64.zip"))!;
            var linuxAsset = release.Assets.FirstOrDefault(x => x.FileName.EndsWith("linux-x64.zip"))!;

            if (windowsAsset is not null)
            {
                _logger.LogInformation($"Found Windows release {release.TagName}");

                GeneralReleaseJsonModel winRelease = new()
                {
                    SupportedOS = OSEnum.Windows,
                    Version = release.TagName,
                    Description = release.Description,
                    DownloadUrl = new Uri(windowsAsset.DownloadUrl)
                };

                AppRelease.AddOrReplace(OSEnum.Windows, winRelease);
            }

            if (linuxAsset is not null)
            {
                _logger.LogInformation($"Found Linux release {release.TagName}");

                GeneralReleaseJsonModel linRelease = new()
                {
                    SupportedOS = OSEnum.Linux,
                    Version = release.TagName,
                    Description = release.Description,
                    DownloadUrl = new Uri(linuxAsset.DownloadUrl)
                };

                AppRelease.Add(OSEnum.Linux, linRelease);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while getting latest app release");
            _logger.LogError(ex.ToString());
        }
    }
}
