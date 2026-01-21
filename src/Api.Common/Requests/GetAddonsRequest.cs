using System.ComponentModel.DataAnnotations;
using Api.Common.Responses;
using Common.All.Enums;
using Mediator;

namespace Api.Common.Requests;

public sealed class GetAddonsRequest : BaseRequest, IRequest<GetAddonsResponse>
{
    [Range(1, 2)]
    public required GameEnum GameEnum { get; set; }
}