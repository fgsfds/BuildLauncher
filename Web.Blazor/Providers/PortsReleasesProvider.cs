using Common.Entities;
using Common.Enums;
using Common.Helpers;
using Common.Server.Entities;
using Common.Server.Providers;
using CommunityToolkit.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Web.Blazor.Providers;

public sealed partial class PortsReleasesProvider
{
    private readonly ILogger<PortsReleasesProvider> _logger;
    private readonly RepositoriesProvider _repoProvider;
    private readonly HttpClient _httpClient;

    public Dictionary<PortEnum, GeneralReleaseEntity>? WindowsReleases { get; set; }
    public Dictionary<PortEnum, GeneralReleaseEntity>? LinuxReleases { get; set; }

    public PortsReleasesProvider(
        ILogger<PortsReleasesProvider> logger,
        RepositoriesProvider repoProvider,
        HttpClient httpClient
        )
    {
        _repoProvider = repoProvider;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task GetLatestReleasesAsync()
    {
        _logger.LogInformation("Looking for new ports releases");

        var ports = Enum.GetValues<PortEnum>();

        foreach (var port in ports)
        {
            if (port is PortEnum.Stub)
            {
                continue;
            }

            try
            {
                var newRelease = await GetLatestReleaseAsync(port).ConfigureAwait(false);

                WindowsReleases ??= [];
                LinuxReleases ??= [];

                if (newRelease is not null)
                {
                    var doesWinExist = newRelease.TryGetValue(OSEnum.Windows, out var winRelease);

                    if (doesWinExist)
                    {
                        WindowsReleases.AddOrReplace(port, winRelease);
                    } 
                    
                    var doesLinExist = newRelease.TryGetValue(OSEnum.Linux, out var linRelease);

                    if (doesWinExist)
                    {
                        LinuxReleases.AddOrReplace(port, linRelease);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while getting latest release for {port}");
                _logger.LogError(ex.ToString());
            }
        }
    }

    /// <summary>
    /// Get the latest release of the selected port
    /// </summary>
    /// <param name="portEnum">Port</param>
    private async Task<Dictionary<OSEnum, GeneralReleaseEntity>?> GetLatestReleaseAsync(PortEnum portEnum)
    {
        var repo = _repoProvider.GetPortRepo(portEnum);

        Dictionary<OSEnum, GeneralReleaseEntity>? result = null;

        if (portEnum is PortEnum.BuildGDX)
        {
            GeneralReleaseEntity bgdxRelease = new()
            {
                SupportedOS = OSEnum.Windows,
                Description = string.Empty,
                Version = "1.17",
                DownloadUrl = repo.RepoUrl
            };

            return new() { { OSEnum.Windows, bgdxRelease } };
        }

        if (repo.RepoUrl is null)
        {
            return null;
        }

        if (portEnum is PortEnum.VoidSW)
        {
            return null;
        }

        var response = await _httpClient.GetStringAsync(repo.RepoUrl).ConfigureAwait(false);

        if (portEnum is PortEnum.EDuke32)
        {
            var edukeRelease = EDuke32Hack(response);
            return edukeRelease is null ? null : new() { { OSEnum.Windows, edukeRelease } };
        }

        var allReleases = JsonSerializer.Deserialize(response, GitHubReleaseEntityContext.Default.ListGitHubReleaseEntity)
            ?? ThrowHelper.ThrowFormatException<List<GitHubReleaseEntity>>("Error while deserializing GitHub releases");

        var releases = allReleases.Where(static x => x.IsDraft is false && x.IsPrerelease is false);

        if (releases is null)
        {
            return null;
        }

        if (repo.WindowsReleasePredicate is not null)
        {
            foreach (var release in releases)
            {
                var winAss = release.Assets.FirstOrDefault(x => repo.WindowsReleasePredicate(x));

                if (winAss is not null)
                {
                    GeneralReleaseEntity portRelease = new()
                    {
                        SupportedOS = OSEnum.Windows,
                        Description = release.Description,
                        Version = GetVersion(portEnum, release, winAss),
                        DownloadUrl = winAss is null ? null : new(winAss.DownloadUrl),
                    };

                    result ??= [];
                    result.Add(OSEnum.Windows, portRelease);

                    _logger.LogInformation($"Latest Windows release for {portEnum}: {portRelease.Version}");

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
                    GeneralReleaseEntity portRelease = new()
                    {
                        SupportedOS = OSEnum.Linux,
                        Description = release.Description,
                        Version = GetVersion(portEnum, release, linAss),
                        DownloadUrl = linAss is null ? null : new(linAss.DownloadUrl),
                    };

                    result ??= [];
                    result.Add(OSEnum.Linux, portRelease);

                    _logger.LogInformation($"Latest Linux release for {portEnum}: {portRelease.Version}");

                    break;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Get port version
    /// </summary>
    private string GetVersion(PortEnum portEnum, GitHubReleaseEntity release, GitHubReleaseAsset asset)
    {
        string version;

        if (portEnum is PortEnum.NotBlood)
        {
            version = asset.UpdatedDate.ToUniversalTime().ToString();
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
    private GeneralReleaseEntity? EDuke32Hack(string response)
    {
        var regex = EDuke32WindowsReleaseRegex();
        var fileName = regex.Matches(response).FirstOrDefault();

        if (fileName is null)
        {
            return null;
        }

        var regexVersion = EDuke32VersionRegex();
        var version = regexVersion.Matches(fileName.ToString()).FirstOrDefault();

        if (version is null)
        {
            return null;
        }

        GeneralReleaseEntity release = new()
        {
            SupportedOS = OSEnum.Windows,
            Description = string.Empty,
            Version = "r" + version,
            DownloadUrl = new($"https://dukeworld.com/eduke32/synthesis/latest/{fileName}")
        };

        _logger.LogInformation($"Latest release for {nameof(PortEnum.EDuke32)}: {release.Version}");

        return release;
    }

    [GeneratedRegex("eduke32_win64_2[^\"]*")]
    private partial Regex EDuke32WindowsReleaseRegex();

    [GeneratedRegex(@"(?<=\-)(\d*)(?=\-)")]
    private partial Regex EDuke32VersionRegex();
}
