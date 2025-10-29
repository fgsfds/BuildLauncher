using System.Collections.Concurrent;
using System.Text.Json;
using Common.All.Enums;
using Common.All.Interfaces;
using Common.All.Serializable.Downloadable;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Tools.Providers;

public sealed class ToolsReleasesProvider : IReleaseProvider<ToolEnum>
{
    private readonly ConcurrentDictionary<ToolEnum, Dictionary<OSEnum, GeneralReleaseJsonModel>?> _releases = [];

    private static readonly SemaphoreSlim _semaphore = new(1);

    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;

    public ToolsReleasesProvider(
        ILogger logger,
        HttpClient httpClient
        )
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Get the latest release of the selected tool
    /// </summary>
    /// <param name="toolEnum">Tool</param>
    public async Task<Dictionary<OSEnum, GeneralReleaseJsonModel>?> GetLatestReleaseAsync(ToolEnum toolEnum)
    {
        try
        {
            _logger.LogInformation($"Looking for new {toolEnum} release.");

            var repo = ToolsRepositoriesProvider.GetToolRepo(toolEnum);

            if (repo.RepoUrl is null)
            {
                return null;
            }

            await _semaphore.WaitAsync().ConfigureAwait(false);

            if (_releases.TryGetValue(toolEnum, out var existingRelease))
            {
                return existingRelease;
            }

            var response = await _httpClient.GetStringAsync(repo.RepoUrl).ConfigureAwait(false);

            var allReleases = JsonSerializer.Deserialize(response, GitHubReleaseEntityContext.Default.ListGitHubReleaseJsonModel)
                ?? ThrowHelper.ThrowFormatException<List<GitHubReleaseJsonModel>>("Error while deserializing GitHub releases");

            var releases = allReleases.Where(static x => !x.IsDraft && !x.IsPrerelease);

            if (releases is null)
            {
                return null;
            }

            Dictionary<OSEnum, GeneralReleaseJsonModel>? result = null;

            if (repo.WindowsReleasePredicate is not null)
            {
                foreach (var release in releases)
                {
                    var winAss = release.Assets.FirstOrDefault(x => repo.WindowsReleasePredicate(x));

                    if (winAss is not null)
                    {
                        GeneralReleaseJsonModel toolRelease = new()
                        {
                            SupportedOS = OSEnum.Windows,
                            Description = release.Description,
                            Version = GetVersion(toolEnum, release, winAss),
                            DownloadUrl = winAss is null ? null : new(winAss.DownloadUrl),
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
                        GeneralReleaseJsonModel toolRelease = new()
                        {
                            SupportedOS = OSEnum.Linux,
                            Description = release.Description,
                            Version = GetVersion(toolEnum, release, linAss),
                            DownloadUrl = linAss is null ? null : new(linAss.DownloadUrl),
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
        finally
        {
            if (_semaphore.CurrentCount == 0)
            {
                _ = _semaphore.Release();
            }
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
