using Common.All.Enums;
using Common.All.Serializable.Downloadable;

namespace Api.Common.Responses;

public sealed class GetPortsReleasesResponse
{
    public required Dictionary<PortEnum, GeneralReleaseJsonModel> PortsReleases { get; set; }
}