﻿using System.Text.Json;
using Common.All.Enums;
using Common.All.Helpers;
using Common.All.Serializable.Downloadable;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Common.All.Providers;

public sealed class RepoAppReleasesProvider
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;

    private Dictionary<OSEnum, GeneralReleaseJsonModel>? _appRelease = null;

    public RepoAppReleasesProvider(
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
    public async Task<Dictionary<OSEnum, GeneralReleaseJsonModel>?> GetLatestReleaseAsync(bool includePreReleases)
    {
        try
        {
            if (_appRelease is not null)
            {
                return _appRelease;
            }

            _logger.LogInformation("Looking for new app release");

            using var response = await _httpClient.GetAsync(Consts.GitHubReleases, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error while getting releases" + Environment.NewLine + response.StatusCode);
                return null;
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

            _appRelease = [];

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

                _appRelease.AddOrReplace(OSEnum.Windows, winRelease);
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

                _appRelease.Add(OSEnum.Linux, linRelease);
            }

            return _appRelease;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error while getting latest app release");
            return null;
        }
    }
}
