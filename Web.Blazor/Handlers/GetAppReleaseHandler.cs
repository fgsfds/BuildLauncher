using Api.Common.Requests;
using Api.Common.Responses;
using Common.All.Providers;
using MediatR;

namespace Web.Blazor.Handlers;

public sealed class GetAppReleaseHandler : IRequestHandler<GetAppReleaseRequest, GetAppReleaseResponse?>
{
    private readonly RepoAppReleasesProvider _appReleasesProvider;

    public GetAppReleaseHandler(RepoAppReleasesProvider appReleasesProvider)
    {
        _appReleasesProvider = appReleasesProvider;
    }

    public async Task<GetAppReleaseResponse?> Handle(GetAppReleaseRequest request, CancellationToken cancellationToken)
    {
        var releases = await _appReleasesProvider.GetLatestReleaseAsync(false);

        if (releases?.TryGetValue(request.OSEnum, out var release) is not true)
        {
            return null;
        }

        GetAppReleaseResponse response = new()
        {
            AppRelease = release
        };

        return response;
    }
}