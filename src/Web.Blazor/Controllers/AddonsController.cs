using Api.Common.Requests;
using Api.Common.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Web.Blazor.Controllers;

[ApiController]
[Route("api/addons")]
internal sealed class ReleasesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReleasesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetAddonsResponse>> GetAddons(GetAddonsRequest request)
    {
        var response = await _mediator.Send(request);

        if (response is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return Ok(response);
    }

    [HttpGet("ratings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetRatingsResponse>> GetRating(GetRatingsRequest request)
    {
        var response = await _mediator.Send(request);

        if (response is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return Ok(response);
    }
}
