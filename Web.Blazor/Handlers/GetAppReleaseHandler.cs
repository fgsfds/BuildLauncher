using Api.Common.Requests;
using Api.Common.Responses;
using MediatR;
using Web.Blazor.Providers;

namespace Web.Blazor.Handlers;

public sealed class GetAppReleaseHandler : IRequestHandler<GetAppReleaseRequest, GetAppReleaseResponse?>
{
    private readonly AppReleasesProvider _appReleasesProvider;

    public GetAppReleaseHandler(AppReleasesProvider appReleasesProvider)
    {
        _appReleasesProvider = appReleasesProvider;
    }

    public Task<GetAppReleaseResponse?> Handle(GetAppReleaseRequest request, CancellationToken cancellationToken)
    {
        _ = _appReleasesProvider.AppRelease.TryGetValue(request.OSEnum, out var release);

        if (release is null)
        {
            return Task.FromResult<GetAppReleaseResponse?>(null);
        }

        GetAppReleaseResponse response = new()
        {
            AppRelease = release
        };

        return Task.FromResult<GetAppReleaseResponse?>(response);
    }
}