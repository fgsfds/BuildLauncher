using Api.Common.Requests;
using Api.Common.Responses;
using Common.Common.Interfaces;
using Common.Entities;
using Common.Enums;
using MediatR;

namespace Web.Blazor.Handlers;

public sealed class GetPortsReleasesHandler : IRequestHandler<GetPortsReleasesRequest, GetPortsReleasesResponse?>
{
    private readonly IRetriever<Dictionary<PortEnum, GeneralReleaseEntity>?> _portsReleasesRetriever;

    public GetPortsReleasesHandler(IRetriever<Dictionary<PortEnum, GeneralReleaseEntity>?> portsReleasesProvider)
    {
        _portsReleasesRetriever = portsReleasesProvider;
    }

    public Task<GetPortsReleasesResponse?> Handle(GetPortsReleasesRequest request, CancellationToken cancellationToken)
    {
        Dictionary<PortEnum, GeneralReleaseEntity>? releases = [];

        if (request.OSEnum is OSEnum.Windows)
        {
            //releases = _portsReleasesRetriever.WindowsReleases;
        }
        else if (request.OSEnum is OSEnum.Linux)
        {
            //releases = _portsReleasesRetriever.LinuxReleases;
        }

        if (releases is null)
        {
            return Task.FromResult<GetPortsReleasesResponse?>(null);
        }

        GetPortsReleasesResponse response = new()
        {
            PortsReleases = releases
        };

        return Task.FromResult<GetPortsReleasesResponse?>(response);
    }
}