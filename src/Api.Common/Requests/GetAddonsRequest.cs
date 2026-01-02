using Api.Common.Responses;
using Common.All.Enums;
using Mediator;

namespace Api.Common.Requests;

public sealed class GetAddonsRequest : /*BaseRequest,*/ IRequest<GetAddonsResponse>
{
    public required GameEnum GameEnum { get; set; }
}