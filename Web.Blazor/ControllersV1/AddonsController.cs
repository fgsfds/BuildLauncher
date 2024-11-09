//using Common.Entities;
//using Common.Enums;
//using Microsoft.AspNetCore.Mvc;
//using Web.Blazor.Providers;

//namespace Web.Blazor.ControllersV1;

//[Obsolete]
//[ApiController]
//[Route("api/addons")]
//public sealed class AddonsController : ControllerBase
//{
//    private readonly AddonsProvider _addonsProvider;

//    public AddonsController(AddonsProvider addonsProvider)
//    {
//        _addonsProvider = addonsProvider;
//    }

//    [Obsolete]
//    [HttpGet("{GameEnum}")]
//    public List<DownloadableAddonEntity> GetAddons(GameEnum gameEnum) => _addonsProvider.GetAddons(gameEnum);

//    [Obsolete]
//    [HttpGet("ping")]
//    public List<DownloadableAddonEntity> Ping() => _addonsProvider.GetAddons(GameEnum.Duke3D, true);


//    [Obsolete]
//    [HttpGet("scores")]
//    public Dictionary<string, int> GetScores() => [];


//    [Obsolete]
//    [HttpPut("scores/change")]
//    public int ChangeScore([FromBody] Tuple<string, sbyte> message) => 0;


//    [Obsolete]
//    [HttpGet("rating")]
//    public Dictionary<string, decimal> GetRating() => _addonsProvider.GetRating();


//    [Obsolete]
//    [HttpPut("rating/change")]
//    public decimal ChangeRating([FromBody] Tuple<string, sbyte, bool> message) => _addonsProvider.ChangeRating(message.Item1, message.Item2, message.Item3);


//    [Obsolete]
//    [HttpPut("installs/add")]
//    public int IncreaseNumberOfInstalls([FromBody] string addonId) => _addonsProvider.IncreaseNumberOfInstalls(addonId);


//    [Obsolete]
//    [HttpPost("add")]
//    public IResult AddAddonToDatabase([FromBody] Tuple<AddonsJsonEntity, string> message)
//    {
//        var apiPassword = Environment.GetEnvironmentVariable("ApiPass")!;

//        if (!apiPassword.Equals(message.Item2))
//        {
//            return Results.Forbid();
//        }

//        var result = _addonsProvider.AddAddonToDatabase(message.Item1);

//        return result ? Results.Ok() : Results.BadRequest();
//    }
//}
