using Common.All.Enums;

namespace Api.Common.Requests;

public sealed class GetAppReleaseRequest : BaseRequest
{
    public required OSEnum OSEnum { get; set; }
}