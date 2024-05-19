using Common;
using Common.Enums;
using Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Superheater.Web.Server.Providers;

namespace Superheater.Web.Server.Controllers
{
    [ApiController]
    [Route("api/addons")]
    public sealed class AddonsController : ControllerBase
    {
        private readonly AddonsProvider _addonsProvider;

        public AddonsController(AddonsProvider addonsProvider)
        {
            _addonsProvider = addonsProvider;
        }

        [HttpGet("{GameEnum}")]
        public async Task<List<DownloadableAddonEntity>> GetAppReleaseAsync(GameEnum gameEnum) => await _addonsProvider.GetAddonsListAsync(gameEnum).ConfigureAwait(false);
    }
}
