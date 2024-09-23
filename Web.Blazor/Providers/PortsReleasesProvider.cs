using Common.Entities;
using Common.Enums;
using Common.Helpers;
using Common.Providers;
using Common.Releases;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Web.Blazor.Providers;

public sealed partial class PortsReleasesProvider
{
    private readonly ILogger<PortsReleasesProvider> _logger;
    private readonly RepositoriesProvider _repoProvider;
    private readonly HttpClient _httpClient;

    public Dictionary<PortEnum, GeneralReleaseEntity> PortsReleases { get; set; }

    public PortsReleasesProvider(
        ILogger<PortsReleasesProvider> logger,
        RepositoriesProvider repoProvider,
        HttpClient httpClient
        )
    {
        _repoProvider = repoProvider;
        _logger = logger;
        _httpClient = httpClient;

        PortsReleases = [];
    }

    public async Task GetLatestReleasesAsync()
    {
        _logger.LogInformation("Looking for new ports releases");

        var ports = Enum.GetValues<PortEnum>();

        foreach (var port in ports)
        {
            try
            {
                var newRelease = await GetLatestReleaseAsync(port).ConfigureAwait(false);

                if (newRelease is not null)
                {
                    var doesExist = PortsReleases.TryGetValue(port, out _);

                    if (doesExist)
                    {
                        PortsReleases[port] = newRelease;
                    }
                    else
                    {
                        PortsReleases.Add(port, newRelease);
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
    private async Task<GeneralReleaseEntity?> GetLatestReleaseAsync(PortEnum portEnum)
    {
        var repo = _repoProvider.GetPortRepo(portEnum);

        if (portEnum is PortEnum.BuildGDX)
        {
            GeneralReleaseEntity bgdxRelease = new()
            {
                Description = string.Empty,
                Version = "1.17",
                WindowsDownloadUrl = repo.RepoUrl,
                LinuxDownloadUrl = null
            };

            return bgdxRelease;
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
            return EDuke32Hack(response);
        }

        var releases = JsonSerializer.Deserialize(response, GitHubReleaseContext.Default.ListGitHubReleaseEntity)
            ?? ThrowHelper.Exception<List<GitHubReleaseEntity>>("Error while deserializing GitHub releases");

        var release = releases.FirstOrDefault(static x => x.IsDraft is false && x.IsPrerelease is false);

        if (release is null)
        {
            return null;
        }

        if (repo.WindowsReleasePredicate is null)
        {
            return null;
        }

        var zip = release.Assets.FirstOrDefault(repo.WindowsReleasePredicate);

        if (zip is null)
        {
            return null;
        }

        GeneralReleaseEntity portRelease = new()
        {
            Description = release.Description,
            Version = GetVersion(portEnum, release, zip),
            WindowsDownloadUrl = new(zip.DownloadUrl),
            LinuxDownloadUrl = null
        };

        _logger.LogInformation($"Latest release for {portEnum}: {portRelease.Version}");

        return portRelease;
    }

    /// <summary>
    /// Get port version
    /// </summary>
    private string GetVersion(PortEnum portEnum, GitHubReleaseEntity release, GitHubReleaseAsset zip)
    {
        string version;

        if (portEnum is PortEnum.NotBlood)
        {
            version = zip.UpdatedDate.ToString("dd.MM.yyyy");
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
            Description = string.Empty,
            Version = "r" + version,
            WindowsDownloadUrl = new($"https://dukeworld.com/eduke32/synthesis/latest/{fileName}"),
            LinuxDownloadUrl = null
        };

        _logger.LogInformation($"Latest release for {nameof(PortEnum.EDuke32)}: {release.Version}");

        return release;
    }

    [GeneratedRegex("eduke32_win64_2[^\"]*")]
    private partial Regex EDuke32WindowsReleaseRegex();

    [GeneratedRegex(@"(?<=\-)(\d*)(?=\-)")]
    private partial Regex EDuke32VersionRegex();
}
