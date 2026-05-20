//using Api.Common.Requests;
//using Api.Common.Responses;
//using Microsoft.AspNetCore.Mvc;

//namespace Web.API.Controllers;

//[ApiController]
//[Route("api/addons")]
//internal sealed class ReleasesController : ControllerBase
//{
//    [HttpGet()]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    public async Task<ActionResult<GetAddonsResponse>> GetAddons(GetAddonsRequest request)
//    {
//        var response = await _mediator.Send(request);

//        if (response is null)
//        {
//            return StatusCode(500);
//        }

//        return Ok(response);
//    }

//    [HttpGet("ratings")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    public async Task<ActionResult<GetRatingsResponse>> GetRating(GetRatingsRequest request)
//    {
//        var response = await _mediator.Send(request);

//        if (response is null)
//        {
//            return StatusCode(500);
//        }

//        return Ok(response);
//    }
//}
