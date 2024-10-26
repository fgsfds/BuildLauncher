using Api.Common.Requests;
using Api.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web.Blazor.ControllersV2;

[ApiController]
[Route("api2/addons")]
public sealed class ReleasesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReleasesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetAddonsResponse>> GetAddonsMediator(GetAddonsRequest request)
    {
        var response = await _mediator.Send(request);

        if (response is null)
        {
            return StatusCode(500);
        }

        return Ok(response);
    }
}
