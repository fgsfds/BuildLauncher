using Api.Common.Requests;
using Api.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web.Blazor.ControllersV2;

[ApiController]
[Route("api2/addons")]
public sealed class AddonsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AddonsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetAddonsResponse>> GetAddonsMediator(GetAddonsRequest request)
    {
        var result = await _mediator.Send(request);

        if (result is null)
        {
            return StatusCode(500);
        }

        GetAddonsResponse response = new() { AddonsList = result.AddonsList };

        return Ok(response);
    }
}
