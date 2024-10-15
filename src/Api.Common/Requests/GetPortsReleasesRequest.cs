using Api.Common.Responses;
using Common.Enums;
using MediatR;

namespace Api.Common.Requests;

public sealed class GetPortsReleasesRequest : IRequest<GetPortsReleasesResponse?>
{
    public required OSEnum OSEnum { get; set; }
}