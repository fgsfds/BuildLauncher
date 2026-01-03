using Api.Common.Requests;
using Api.Common.Responses;
using Common.All;
using Common.All.Enums;
using Common.All.Interfaces;
using Mediator;

namespace Web.Blazor.Handlers;

internal sealed class GetPortsReleasesHandler : IRequestHandler<GetPortsReleasesRequest, GetPortsReleasesResponse?>
{
    private readonly IReleaseProvider<PortEnum> _portsReleasesRetriever;

    public GetPortsReleasesHandler(IReleaseProvider<PortEnum> portsReleasesProvider)
    {
        _portsReleasesRetriever = portsReleasesProvider;
    }

    public ValueTask<GetPortsReleasesResponse?> Handle(GetPortsReleasesRequest request, CancellationToken cancellationToken)
    {
        Dictionary<PortEnum, GeneralRelease>? releases = [];

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
            return ValueTask.FromResult<GetPortsReleasesResponse?>(null);
        }

        GetPortsReleasesResponse response = new()
        {
            PortsReleases = releases
        };

        return ValueTask.FromResult<GetPortsReleasesResponse?>(response);
    }
}