using Api.Common.Requests;
using Api.Common.Responses;
using MediatR;
using Web.Blazor.Providers;

namespace Web.Blazor.Handlers;

public sealed class GetAddonsHandler : IRequestHandler<GetAddonsRequest, GetAddonsResponse>
{
    private readonly DatabaseAddonsProvider _addonsProvider;

    public GetAddonsHandler(DatabaseAddonsProvider addonsProvider)
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