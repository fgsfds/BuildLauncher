using Api.Common.Requests;
using Api.Common.Responses;
using MediatR;
using Web.Blazor.Providers;

namespace Web.Blazor.Handlers;

public sealed class GetRatingsHandler : IRequestHandler<GetRatingsRequest, GetRatingsResponse>
{
    private readonly DatabaseAddonsProvider _addonsProvider;

    public GetRatingsHandler(DatabaseAddonsProvider addonsProvider)
    {
        _addonsProvider = addonsProvider;
    }

    public Task<GetRatingsResponse> Handle(GetRatingsRequest request, CancellationToken cancellationToken)
    {
        var ratings = _addonsProvider.GetRating();

        GetRatingsResponse response = new() 
        { 
            Ratings = ratings
        };

        return Task.FromResult(response);
    }
}