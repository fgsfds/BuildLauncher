using System.Collections.Concurrent;
using System.Text.Json;
using Common.All;
using Common.All.Enums;
using Common.All.Helpers;
using Common.All.Interfaces;
using Common.All.Serializable.Downloadable;
using Microsoft.Extensions.Logging;

namespace Tools.Providers;

public sealed class ToolsReleasesProvider : IReleaseProvider<ToolEnum>
{
    private readonly ConcurrentDictionary<ToolEnum, Dictionary<OSEnum, GeneralRelease>?> _releases = [];
    private readonly ILogger<ToolsReleasesProvider> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ToolsReleasesProvider(
        ILogger<ToolsReleasesProvider> logger,
        IHttpClientFactory httpClientFactory
        )
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Get the latest release of the selected tool
    /// </summary>
    /// <param name="toolEnum">Tool</param>
    public async Task<Dictionary<OSEnum, GeneralRelease>?> GetLatestReleaseAsync(ToolEnum toolEnum)
    {
        try
        {
            _logger.LogInformation($"Looking for new {toolEnum} release.");

            var repo = ToolsRepositoriesProvider.GetToolRepo(toolEnum);

            if (repo.RepoUrl is null)
            {
                return null;
            }

            if (_releases.TryGetValue(toolEnum, out var existingRelease))
            {
                return existingRelease;
            }

            using var httpClient = _httpClientFactory.CreateClient(HttpClientEnum.GitHub.GetDescription());
            using var response = await httpClient.GetAsync(repo.RepoUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var allReleases = JsonSerializer.Deserialize(data, GitHubReleaseEntityContext.Default.ListGitHubReleaseJsonModel)
                ?? throw new FormatException("Error while deserializing GitHub releases");

            var releases = allReleases
                .Where(static x => !x.IsDraft && !x.IsPrerelease)
                .ToList();

            if (releases is null)
            {
                return null;
            }

            Dictionary<OSEnum, GeneralRelease>? result = null;

            if (repo.WindowsReleasePredicate is not null)
            {
                foreach (var release in releases)
                {
                    var winAss = release.Assets.FirstOrDefault(x => repo.WindowsReleasePredicate(x));

                    if (winAss is not null)
                    {
                        GeneralRelease toolRelease = new()
                        {
                            SupportedOS = OSEnum.Windows,
                            Description = release.Description,
                            Version = GetVersion(toolEnum, release, winAss),
                            DownloadUrl = new(winAss.DownloadUrl),
                            Hash = winAss.Digest
                        };

                        result ??= [];
                        result.Add(OSEnum.Windows, toolRelease);

                        _logger.LogInformation($"Latest Windows release for {toolEnum}: {toolRelease.Version}.");

                        break;
                    }
                }
            }

            if (repo.LinuxReleasePredicate is not null)
            {
                foreach (var release in releases)
                {
                    var linAss = release.Assets.FirstOrDefault(x => repo.LinuxReleasePredicate(x));

                    if (linAss is not null)
                    {
                        GeneralRelease toolRelease = new()
                        {
                            SupportedOS = OSEnum.Linux,
                            Description = release.Description,
                            Version = GetVersion(toolEnum, release, linAss),
                            DownloadUrl = new(linAss.DownloadUrl),
                            Hash = linAss.Digest
                        };

                        result ??= [];
                        result.Add(OSEnum.Linux, toolRelease);

                        _logger.LogInformation($"Latest Linux release for {toolEnum}: {toolRelease.Version}.");

                        break;
                    }
                }
            }

            _ = _releases.TryAdd(toolEnum, result);

            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Error while getting latest release for {toolEnum}.");
            return null;
        }
    }

    /// <summary>
    /// Get tool version
    /// </summary>
    private string GetVersion(ToolEnum toolEnum, GitHubReleaseJsonModel release, GitHubReleaseAsset asset)
    {
        if (toolEnum is ToolEnum.XMapEdit or ToolEnum.DOSBlood)
        {
            return asset.UpdatedDate.ToUniversalTime().ToString();
        }

        return release.TagName;
    }
}
