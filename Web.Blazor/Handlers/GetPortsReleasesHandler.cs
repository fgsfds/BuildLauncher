using Api.Common.Requests;
using Api.Common.Responses;
using Common.Entities;
using Common.Enums;
using MediatR;
using Web.Blazor.Providers;

namespace Web.Blazor.Handlers;

public sealed class GetPortsReleasesHandler : IRequestHandler<GetPortsReleasesRequest, GetPortsReleasesResponse>
{
    private readonly PortsReleasesProvider _portsReleasesProvider;

    public GetPortsReleasesHandler(PortsReleasesProvider portsReleasesProvider)
    {
        _portsReleasesProvider = portsReleasesProvider;
    }

    public Task<GetPortsReleasesResponse> Handle(GetPortsReleasesRequest request, CancellationToken cancellationToken)
    {
        Dictionary<PortEnum, GeneralReleaseEntity>? releases = [];

        if (request.OSEnum is OSEnum.Windows)
        {
            releases = _portsReleasesProvider.WindowsReleases;
        }
        else if (request.OSEnum is OSEnum.Linux)
        {
            releases = _portsReleasesProvider.LinuxReleases;
        }

        GetPortsReleasesResponse response = new()
        {
            PortsReleases = releases
        };

        return Task.FromResult(response);
    }
}