using ClientCommon.Config;

namespace ClientCommon.Providers
{
    public sealed class PlaytimeProvider
    {
        private readonly Dictionary<string, TimeSpan> _times;
        private readonly ConfigProvider _config;

        public PlaytimeProvider(ConfigProvider config)
        {
            _config = config;

            _times = new(_config.Playtimes, StringComparer.InvariantCultureIgnoreCase);
        }

        public TimeSpan? GetTime(string id)
        {
            if (_times.TryGetValue(id, out var time))
            {
                return time;
            }

            return null;
        }

        public void AddTime(string id, TimeSpan time)
        {
            if (!_times.TryAdd(id, time))
            {
                _times[id] = _times[id].Add(time);
            }

            _config.AddPlaytime(id, time);
        }
    }
}
