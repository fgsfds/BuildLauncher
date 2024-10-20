using Common.Entities;
using Common.Enums;

namespace Api.Common.Responses;

public sealed class GetPortsReleasesResponse
{
    public required Dictionary<PortEnum, GeneralReleaseEntity> PortsReleases { get; set; }
}