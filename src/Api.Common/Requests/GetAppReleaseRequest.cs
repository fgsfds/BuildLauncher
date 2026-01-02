using Api.Common.Responses;
using Common.All.Enums;
using Mediator;

namespace Api.Common.Requests;

public sealed class GetAppReleaseRequest : BaseRequest, IRequest<GetAppReleaseResponse?>
{
    public required OSEnum OSEnum { get; set; }
}