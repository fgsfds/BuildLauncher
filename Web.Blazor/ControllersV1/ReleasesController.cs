using Common.Entities;
using Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Web.Blazor.Providers;

namespace Web.Blazor.ControllersV1;

[Obsolete]
[ApiController]
[Route("api/releases")]
public sealed class ReleasesController : ControllerBase
{
    private readonly AppReleasesProvider _appReleasesProvider;
    private readonly PortsReleasesProvider _portsReleasesProvider;

    public ReleasesController(
        AppReleasesProvider appReleasesProvider,
        PortsReleasesProvider portsReleasesProvider
        )
    {
        _appReleasesProvider = appReleasesProvider;
        _portsReleasesProvider = portsReleasesProvider;
    }

    [Obsolete]
    [HttpGet("app")]
    public GeneralReleaseEntityObsolete? GetLatestAppRelease()
    {
        if (_appReleasesProvider.AppRelease?.TryGetValue(OSEnum.Windows, out var winRelease) ?? false)
        {
            return new GeneralReleaseEntityObsolete()
            {
                Description = winRelease.Description,
                Version = winRelease.Version,
                WindowsDownloadUrl = winRelease.DownloadUrl,
                LinuxDownloadUrl = null
            };
        }

        return null;
    }

    [Obsolete]
    [HttpGet("ports")]
    public Dictionary<PortEnum, GeneralReleaseEntityObsolete> GetLatestPortsReleases()
    {
        Dictionary<PortEnum, GeneralReleaseEntityObsolete> result = [];
        var releases = _portsReleasesProvider.WindowsReleases;

        if (releases is null)
        {
            return result;
        }

        foreach (var release in releases)
        {
            result.Add(release.Key, new GeneralReleaseEntityObsolete()
            {
                Description = release.Value.Description,
                Version = release.Value.Version,
                WindowsDownloadUrl = release.Value.DownloadUrl,
                LinuxDownloadUrl = null
            });
        }

        return result;
    }

    [Obsolete]
    [HttpGet("tools")]
    public Dictionary<ToolEnum, GeneralReleaseEntityObsolete> GetLatestToolsReleases() => [];
}
