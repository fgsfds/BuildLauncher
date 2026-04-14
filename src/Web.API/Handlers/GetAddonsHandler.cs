using Api.Common.Requests;
using Api.Common.Responses;
using MediatR;
using Web.API.Providers;

namespace Web.API.Handlers;

internal sealed class GetAddonsHandler : IRequestHandler<GetAddonsRequest, GetAddonsResponse>
{
    private readonly DatabaseAddonsRetriever _addonsProvider;

    public GetAddonsHandler(DatabaseAddonsRetriever addonsProvider)
    {
        _addonsProvider = addonsProvider;
    }

    public Task<GetAddonsResponse> Handle(GetAddonsRequest request, CancellationToken cancellationToken)
    {
        var addons = _addonsProvider.GetAddons(request.GameEnum);

        GetAddonsResponse response = new()
        {
            AddonsList = addons
        };

        return Task.FromResult(response);
    }
}