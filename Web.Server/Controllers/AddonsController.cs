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
        public List<DownloadableAddonEntity> GetAddons(GameEnum gameEnum) => _addonsProvider.GetAddons(gameEnum);


        [HttpGet("scores")]
        public Dictionary<string, int> GetScores() => _addonsProvider.GetScores();


        [HttpPut("scores/change")]
        public int ChangeScore([FromBody] Tuple<string, sbyte> message) => _addonsProvider.ChangeScore(message.Item1, message.Item2);


        [HttpPut("installs/add")]
        public int IncreaseNumberOfInstalls([FromBody] string addonId) => _addonsProvider.IncreaseNumberOfInstalls(addonId);


        [HttpPost("add")]
        public IResult AddAddonToDatabase([FromBody] Tuple<AddonsJsonEntity, string> message)
        {
            var apiPassword = Environment.GetEnvironmentVariable("ApiPass")!;

            if (!apiPassword.Equals(message.Item2))
            {
                return Results.Forbid();
            }

            var result = _addonsProvider.AddAddonToDatabase(message.Item1);

            return result ? Results.Ok() : Results.BadRequest();
        }
    }
}
