using Api.Common.Responses;
using Common.All.Enums;
using Mediator;

namespace Api.Common.Requests;

public sealed class GetPortsReleasesRequest : BaseRequest, IRequest<GetPortsReleasesResponse?>
{
    /// <summary>
    /// OS enum. 1 = Windows, 2 = Linux.
    /// </summary>
    public required OSEnum OSEnum { get; set; }
}