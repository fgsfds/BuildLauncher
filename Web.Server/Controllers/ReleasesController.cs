using Common.Entities;
using Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Web.Server.Providers;

namespace Web.Server.Controllers
{
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

        [HttpGet("app")]
        public GeneralReleaseEntity? GetAppRelease() => _appReleasesProvider.AppRelease;

        [HttpGet("ports")]
        public Dictionary<PortEnum, GeneralReleaseEntity> GetPortsReleases() => _portsReleasesProvider.PortsReleases;

        [HttpGet("tools")]
        public Dictionary<ToolEnum, GeneralReleaseEntity> GetToolsReleases() => _toolsReleasesProvider.ToolsReleases;
    }
}
