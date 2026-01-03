using Common.All;
using Common.All.Enums;

namespace Api.Common.Responses;

public sealed class GetPortsReleasesResponse
{
    public required Dictionary<PortEnum, GeneralRelease> PortsReleases { get; set; }
}