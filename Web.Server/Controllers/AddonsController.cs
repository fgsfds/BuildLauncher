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


        [HttpGet("scores")]
        public Dictionary<string, int> GetScores() => _addonsProvider.GetScores();


        [HttpPut("scores/change")]
        public int ChangeRating([FromBody] Tuple<string, sbyte> message) => _addonsProvider.ChangeAddonScore(message.Item1, message.Item2);


        [HttpPut("installs/add")]
        public int AddNumberOfInstalls([FromBody] string addonId) => _addonsProvider.IncreaseAddonInstallsCount(addonId);


        [HttpPost("report")]
        public void ReportAddon([FromBody] Tuple<string, string> message) => _addonsProvider.AddReport(message.Item1, message.Item2);
    }
}
