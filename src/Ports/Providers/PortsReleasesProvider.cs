using Common.Client.Api;
using Common.Entities;
using Common.Enums;

namespace Ports.Providers;

/// <summary>
/// Class that provides releases from ports' repositories
/// </summary>
public sealed class PortsReleasesProvider
{
    private readonly ApiInterface _apiInterface;
    private readonly SemaphoreSlim _semaphore = new(1);
    private Dictionary<PortEnum, GeneralReleaseEntity>? _releases;

    public PortsReleasesProvider(ApiInterface apiInterface)
    {
        _apiInterface = apiInterface;
    }

    /// <summary>
    /// Get the latest release of the selected port
    /// </summary>
    /// <param name="portEnum">Port enum</param>
    public async Task<GeneralReleaseEntity?> GetLatestReleaseAsync(PortEnum portEnum)
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

    public async Task<Dictionary<PortEnum, GeneralReleaseEntity>?> GetReleasesAsync()
    {
        return await _apiInterface.GetLatestPortsReleasesAsync().ConfigureAwait(false);
    }
}
