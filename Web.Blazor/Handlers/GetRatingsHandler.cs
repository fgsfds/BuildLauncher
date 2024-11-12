using Api.Common.Requests;
using Api.Common.Responses;
using MediatR;
using Web.Blazor.Providers;

namespace Web.Blazor.Handlers;

public sealed class GetRatingsHandler : IRequestHandler<GetRatingsRequest, GetRatingsResponse>
{
    private readonly DatabaseAddonsRetriever _addonsProvider;

    public GetRatingsHandler(DatabaseAddonsRetriever addonsProvider)
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