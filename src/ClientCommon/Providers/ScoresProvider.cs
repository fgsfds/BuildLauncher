using ClientCommon.API;
using ClientCommon.Config;
using Common.Helpers;

namespace ClientCommon.Providers;

public sealed class ScoresProvider
{
    private readonly ApiInterface _apiInterface;
    private readonly IConfigProvider _config;
    private Dictionary<string, int>? _cache;

    public ScoresProvider(
        ApiInterface apiInterface,
        IConfigProvider config)
    {
        _apiInterface = apiInterface;
        _config = config;

        _ = CreateCacheAsync();
    }


    public Dictionary<string, int>? GetScores()
    {
        return _cache;
    }


    public int? GetScore(string id)
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
        var cache = await _apiInterface.GetScoresAsync();

        _cache = cache;
    }

    public async Task ChangeScoreAsync(string addonId, bool isUpvote)
    {
        _cache.ThrowIfNull();

        var increment = GetIncrement(addonId, isUpvote);

        var newScore = await _apiInterface.ChangeScoreAsync(addonId, increment).ConfigureAwait(false);

        if (newScore is null)
        {
            return;
        }

        _cache[addonId] = newScore.Value;

        _config.AddScore(addonId, isUpvote);
    }

    private sbyte GetIncrement(string addonId, bool needToUpvote)
    {
        sbyte increment = 0;

        var doesEntryExist = _config.Scores.TryGetValue(addonId, out var isUpvote);

        if (doesEntryExist)
        {
            if (isUpvote && needToUpvote)
            {
                increment = -1;
            }
            else if (isUpvote && !needToUpvote)
            {
                increment = -2;
            }
            else if (!isUpvote && needToUpvote)
            {
                increment = 2;
            }
            else if (!isUpvote && !needToUpvote)
            {
                increment = 1;
            }
        }
        else
        {
            if (needToUpvote)
            {
                increment = 1;
            }
            else
            {
                increment = -1;
            }
        }

        return increment;
    }
}
