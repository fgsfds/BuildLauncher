using Api.Common.Requests;
using Api.Common.Responses;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Web.Blazor.Controllers;

[ApiController]
[Route("api/addons")]
internal sealed class AddonsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AddonsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<Results<Ok<GetAddonsResponse>, InternalServerError>> GetAddons(GetAddonsRequest request)
    {
        var response = await _mediator.Send(request);

        if (response is null)
        {
            return TypedResults.InternalServerError();
        }

        return TypedResults.Ok(response);
    }

    [HttpGet("ratings")]
    public async Task<Results<Ok<GetRatingsResponse>, InternalServerError>> GetRatings(GetRatingsRequest request)
    {
        var response = await _mediator.Send(request);

        if (response is null)
        {
            return TypedResults.InternalServerError();
        }

        return TypedResults.Ok(response);
    }
}
