using Common.Client.Interfaces;
using Common.Common.Serializable.Downloadable;
using Common.Enums;

namespace Ports.Providers;

/// <summary>
/// Class that provides releases from ports' repositories
/// </summary>
public sealed class PortsReleasesProvider
{
    private readonly IApiInterface _apiInterface;
    private readonly SemaphoreSlim _semaphore = new(1);
    private Dictionary<PortEnum, GeneralReleaseJsonModel>? _releases;

    public PortsReleasesProvider(IApiInterface apiInterface)
    {
        _apiInterface = apiInterface;
    }

    /// <summary>
    /// Get the latest release of the selected port
    /// </summary>
    /// <param name="portEnum">Port enum</param>
    public async Task<GeneralReleaseJsonModel?> GetLatestReleaseAsync(PortEnum portEnum)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        if (_releases is null)
        {
            _releases = await GetReleasesAsync().ConfigureAwait(false);
        }

        _ = _semaphore.Release();

        if (_releases is null)
        {
            return null;
        }

        var hasRelease = _releases.TryGetValue(portEnum, out var release);

        return hasRelease ? release : null;
    }

    public Task<Dictionary<PortEnum, GeneralReleaseJsonModel>?> GetReleasesAsync() => _apiInterface.GetLatestPortsReleasesAsync();
}
