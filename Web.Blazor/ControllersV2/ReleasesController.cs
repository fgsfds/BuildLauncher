using Api.Common.Requests;
using Api.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web.Blazor.ControllersV2;

[ApiController]
[Route("api/releases")]
public sealed class AddonsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AddonsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("ports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetPortsReleasesResponse>> GetPortsReleasesMediator(GetPortsReleasesRequest request)
    {
        var response = await _mediator.Send(request);

        if (response is null)
        {
            return StatusCode(500);
        }

        return Ok(response);
    }

    [HttpGet("app")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetAppReleaseResponse>> GetAppReleaseMediator(GetAppReleaseRequest request)
    {
        var response = await _mediator.Send(request);

        if (response is null)
        {
            return StatusCode(500);
        }

        return Ok(response);
    }
}
