﻿using Common.Client.Interfaces;

namespace Common.Client.Providers;

public sealed class PlaytimeProvider
{
    private readonly Dictionary<string, TimeSpan> _times;
    private readonly IConfigProvider _config;

    public PlaytimeProvider(IConfigProvider config)
    {
        _config = config;

        _times = new(_config.Playtimes, StringComparer.OrdinalIgnoreCase);
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
