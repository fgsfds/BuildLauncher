using Core.Client.Interfaces;

namespace Core.Client.Providers;

/// <summary>
///     Provides addon playtime tracking backed by the configuration provider.
/// </summary>
public sealed class PlaytimeProvider
{
    private readonly IConfigProvider _config;

    /// <summary>
    ///     Local dictionary of accumulated playtimes.
    /// </summary>
    private readonly Dictionary<string, TimeSpan> _times;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PlaytimeProvider" /> class.
    /// </summary>
    /// <param name="config">The configuration provider.</param>
    public PlaytimeProvider(IConfigProvider config)
    {
        _config = config;

        _times = new(_config.Playtimes, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Gets the accumulated playtime for the specified addon.
    /// </summary>
    /// <param name="id">The addon identifier.</param>
    /// <returns>The playtime, or null if not found.</returns>
    public TimeSpan? GetTime(string id)
    {
        if (_times.TryGetValue(id, out var time))
        {
            return time;
        }

        return null;
    }

    /// <summary>
    ///     Adds playtime for the specified addon.
    /// </summary>
    /// <param name="id">The addon identifier.</param>
    /// <param name="time">The playtime duration to add.</param>
    public void AddTime(string id, TimeSpan time)
    {
        if (!_times.TryAdd(id, time))
        {
            _times[id] = _times[id].Add(time);
        }

        _config.AddPlaytime(id, time);
    }
}
