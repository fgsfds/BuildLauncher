using Common.All.Enums;

namespace Api.Common.Requests;

public sealed class GetPortsReleasesRequest : BaseRequest
{
    public required OSEnum OSEnum { get; set; }
}