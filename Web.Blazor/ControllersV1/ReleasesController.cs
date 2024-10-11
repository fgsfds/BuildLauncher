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
    private readonly ToolsReleasesProvider _toolsReleasesProvider;

    public ReleasesController(
        AppReleasesProvider appReleasesProvider,
        PortsReleasesProvider portsReleasesProvider,
        ToolsReleasesProvider toolsReleasesProvider
        )
    {
        _appReleasesProvider = appReleasesProvider;
        _portsReleasesProvider = portsReleasesProvider;
        _toolsReleasesProvider = toolsReleasesProvider;
    }

    [Obsolete]
    [HttpGet("app")]
    public GeneralReleaseEntity? GetLatestAppRelease() => _appReleasesProvider.AppRelease;

    [Obsolete]
    [HttpGet("ports")]
    public Dictionary<PortEnum, GeneralReleaseEntity> GetLatestPortsReleases() => _portsReleasesProvider.PortsReleases;

    [Obsolete]
    [HttpGet("tools")]
    public Dictionary<ToolEnum, GeneralReleaseEntity> GetLatestToolsReleases() => _toolsReleasesProvider.ToolsReleases;
}
