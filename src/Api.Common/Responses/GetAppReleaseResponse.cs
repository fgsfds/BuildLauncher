using Common.Entities;

namespace Api.Common.Responses;

public sealed class GetAppReleaseResponse
{
    public required GeneralReleaseEntity? AppRelease { get; set; }
}