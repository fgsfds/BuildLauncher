using Api.Common.Responses;
using Common.Enums;
using MediatR;

namespace Api.Common.Requests;

public sealed class GetAddonsRequest : IRequest<GetAddonsResponse>
{
    public GameEnum GameEnum { get; set; }
}