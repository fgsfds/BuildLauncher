using Api.Common.Requests;
using Api.Common.Responses;
using Common.All.Enums;
using Common.All.Interfaces;
using Common.All.Serializable.Downloadable;
using MediatR;

namespace Web.Blazor.Handlers;

public sealed class GetPortsReleasesHandler : IRequestHandler<GetPortsReleasesRequest, GetPortsReleasesResponse?>
{
    private readonly IReleaseProvider<PortEnum> _portsReleasesRetriever;

    public GetPortsReleasesHandler(IReleaseProvider<PortEnum> portsReleasesProvider)
    {
        _portsReleasesRetriever = portsReleasesProvider;
    }

    public Task<GetPortsReleasesResponse?> Handle(GetPortsReleasesRequest request, CancellationToken cancellationToken)
    {
        Dictionary<PortEnum, GeneralReleaseJsonModel>? releases = [];

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