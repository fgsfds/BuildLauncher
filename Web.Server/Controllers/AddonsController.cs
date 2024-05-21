using Common.Entities;
using Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Web.Server.Providers;

namespace Web.Server.Controllers
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
        public List<DownloadableAddonEntity> GetDownloadableAddons(GameEnum gameEnum) => _addonsProvider.GetAddonsList(gameEnum);
    }
}
