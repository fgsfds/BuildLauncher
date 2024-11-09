namespace Api.Common.Responses;

public sealed class GetRatingsResponse
{
    public required Dictionary<string, decimal> Ratings { get; set; }
}