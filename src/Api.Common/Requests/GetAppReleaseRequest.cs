using Api.Common.Responses;
using Common.Entities;
using Common.Enums;
using MediatR;

namespace Api.Common.Requests;

public sealed class GetAppReleaseRequest : IRequest<GetAppReleaseResponse?>
{
    public required OSEnum OSEnum { get; set; }
}