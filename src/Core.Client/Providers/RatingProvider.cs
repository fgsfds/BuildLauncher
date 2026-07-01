using Core.Client.Interfaces;

namespace Core.Client.Providers;

/// <summary>
///     Provides addon rating management with local caching and remote synchronization.
/// </summary>
public sealed class RatingProvider
{
    private readonly IApiInterface _apiInterface;

    private readonly IConfigProvider _config;

    /// <summary>
    ///     Local cache of ratings.
    /// </summary>
    private Dictionary<string, decimal>? _cache = null;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RatingProvider" /> class.
    /// </summary>
    /// <param name="apiInterface">The API interface for remote rating operations.</param>
    /// <param name="config">The configuration provider for local rating storage.</param>
    public RatingProvider(
        IApiInterface apiInterface,
        IConfigProvider config
        )
    {
        _apiInterface = apiInterface;
        _config = config;

        _ = CreateCacheAsync();
    }


    /// <summary>
    ///     Gets the rating for the specified addon from the local cache.
    /// </summary>
    /// <param name="id">The addon identifier.</param>
    /// <returns>The rating value, or null if not cached.</returns>
    public decimal? GetRating(string id)
    {
        if (_cache is null)
        {
            return null;
        }

        var hasScore = _cache.TryGetValue(id, out var score);

        return hasScore ? score : null;
    }


    /// <summary>
    ///     Creates the local rating cache by fetching ratings from the remote API.
    /// </summary>
    private async Task CreateCacheAsync()
    {
        var cache = await _apiInterface.GetRatingsAsync().ConfigureAwait(false);

        if (cache is null)
        {
            return;
        }

        _cache = new(cache, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Changes the rating for the specified addon and synchronizes with the remote API.
    /// </summary>
    /// <param name="addonId">The addon identifier.</param>
    /// <param name="rating">The new rating value.</param>
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
