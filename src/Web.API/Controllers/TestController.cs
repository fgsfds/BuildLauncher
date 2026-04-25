using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers;

[ApiController]
[Route("test")]
public class TestController
{
    [HttpGet]
    public string[] Get()
    {
        return ["1", "2"];
    }
}
