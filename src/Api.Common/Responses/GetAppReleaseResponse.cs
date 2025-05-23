using Common.Common.Serializable.Downloadable;

namespace Api.Common.Responses;

public sealed class GetAppReleaseResponse
{
    public required GeneralReleaseJsonModel? AppRelease { get; set; }
}