using Common.All;

namespace Api.Common.Responses;

public sealed class GetAppReleaseResponse
{
    public required GeneralRelease? AppRelease { get; set; }
}