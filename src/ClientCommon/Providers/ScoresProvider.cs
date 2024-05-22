using ClientCommon.API;
using Common.Helpers;

namespace ClientCommon.Providers
{
    public sealed class ScoresProvider
    {
        private readonly ApiInterface _apiInterface;
        private Dictionary<string, int>? _cache;

        public ScoresProvider(ApiInterface apiInterface)
        {
            _apiInterface = apiInterface;

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
            var cache = await _apiInterface.GetScores();

            _cache = cache;
        }

        public void ChangeScore(string id, int newScore)
        {
            _cache.ThrowIfNull();

            _cache[id] = newScore;
        }
    }
}
