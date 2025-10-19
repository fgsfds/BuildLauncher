using Api.Common.Responses;
using Common.All.Enums;
using MediatR;

namespace Api.Common.Requests;

public sealed class GetAddonsRequest : /*BaseRequest,*/ IRequest<GetAddonsResponse>
{
    public required GameEnum GameEnum { get; set; }
}