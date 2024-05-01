using Common.Config;

namespace Common.Providers
{
    public sealed class PlaytimeProvider
    {
        private readonly Dictionary<string, TimeSpan> _times;
        private readonly ConfigEntity _config;

        public PlaytimeProvider(ConfigProvider configProvider)
        {
            _config = configProvider.Config;

            if (_config.Playtimes is null)
            {
                _times = new(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                _times = new(_config.Playtimes, StringComparer.OrdinalIgnoreCase);
            }
        }

        public TimeSpan GetTime(string id)
        {
            if (_times.TryGetValue(id, out var time))
            {
                return time;
            }

            return TimeSpan.Zero;
        }

        public void AddTime(string id, TimeSpan time)
        {
            if (!_times.TryAdd(id, time))
            {
                _times[id] = _times[id].Add(time);
            }

            _config.Playtimes = _times;
        }
    }
}
