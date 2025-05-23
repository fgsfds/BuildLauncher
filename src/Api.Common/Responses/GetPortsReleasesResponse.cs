using Common.Common.Serializable.Downloadable;
using Common.Enums;

namespace Api.Common.Responses;

public sealed class GetPortsReleasesResponse
{
    public required Dictionary<PortEnum, GeneralReleaseJsonModel> PortsReleases { get; set; }
}