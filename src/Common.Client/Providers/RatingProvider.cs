using Common.Client.Interfaces;

namespace Common.Client.Providers;

public sealed class RatingProvider
{
    private readonly IApiInterface _apiInterface;
    private readonly IConfigProvider _config;
    private Dictionary<string, decimal>? _cache = null;

    public RatingProvider(
        IApiInterface apiInterface,
        IConfigProvider config
        )
    {
        _apiInterface = apiInterface;
        _config = config;

        _ = CreateCacheAsync();
    }


    public decimal? GetRating(string id)
    {
        if (_cache is null)
        {
            return null;
        }

        var hasScore = _cache.TryGetValue(id, out var score);

        return hasScore ? score : null;
    }


    private async Task CreateCacheAsync()
    {
        var cache = await _apiInterface.GetRatingsAsync().ConfigureAwait(false);

        if (cache is null)
        {
            return;
        }

        _cache = new(cache, StringComparer.OrdinalIgnoreCase);
    }

    public async Task ChangeScoreAsync(string addonId, byte rating)
    {
        ArgumentNullException.ThrowIfNull(_cache);

        var ratingExists = _config.Rating.TryGetValue(addonId, out var existingRating);

        if (rating == existingRating)
        {
            return;
        }

        sbyte newRating;

        if (ratingExists)
        {
            newRating = (sbyte)(rating - existingRating);
        }
        else
        {
            newRating = (sbyte)rating;
        }

        var newScore = await _apiInterface.ChangeScoreAsync(addonId, newRating, !ratingExists).ConfigureAwait(false);

        if (newScore is null)
        {
            return;
        }

        _cache[addonId] = newScore.Value;

        _config.AddScore(addonId, rating);
    }
}
