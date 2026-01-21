using Api.Common.Requests;
using Api.Common.Responses;
using Common.All.Providers;
using Mediator;

namespace Web.Blazor.Handlers;

internal sealed class GetAppReleaseHandler : IRequestHandler<GetAppReleaseRequest, GetAppReleaseResponse?>
{
    private readonly RepoAppReleasesProvider _appReleasesProvider;

    public GetAppReleaseHandler(RepoAppReleasesProvider appReleasesProvider)
    {
        _appReleasesProvider = appReleasesProvider;
    }

    public async ValueTask<GetAppReleaseResponse?> Handle(GetAppReleaseRequest request, CancellationToken cancellationToken)
    {
        var releases = await _appReleasesProvider.GetLatestReleaseAsync(false).ConfigureAwait(false);

        if (releases?.TryGetValue(request.OSEnum, out var release) is not true)
        {
            return null;
        }

        return new()
        {
            AppRelease = release
        };
    }
}