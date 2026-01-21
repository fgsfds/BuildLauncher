using Api.Common.Requests;
using Api.Common.Responses;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Web.Blazor.Controllers;

[ApiController]
[Route("api/releases")]
internal sealed class ReleasesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReleasesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns a list of ports and their latest releases.
    /// </summary>
    [HttpGet("ports")]
    public async Task<Results<Ok<GetPortsReleasesResponse>, InternalServerError>> GetPortsReleases(GetPortsReleasesRequest request)
    {
        var response = await _mediator.Send(request);

        if (response is null)
        {
            return TypedResults.InternalServerError();
        }

        return TypedResults.Ok(response);
    }

    /// <summary>
    /// Returns the latest BuildLauncher release.
    /// </summary>
    [HttpGet("app")]
    public async Task<Results<Ok<GetAppReleaseResponse>, InternalServerError>> GetLatestAppRelease(GetAppReleaseRequest request)
    {
        var response = await _mediator.Send(request);

        if (response is null)
        {
            return TypedResults.InternalServerError();
        }

        return TypedResults.Ok(response);
    }
}
