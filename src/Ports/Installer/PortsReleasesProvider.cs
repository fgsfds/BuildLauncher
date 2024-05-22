using ClientCommon.API;
using ClientCommon.Providers;
using Common.Entities;
using Common.Enums;

namespace Ports.Installer
{
    /// <summary>
    /// Class that provides releases from ports' repositories
    /// </summary>
    public sealed class PortsReleasesProvider
    {
        private readonly ApiInterface _apiInterface;

        private Dictionary<PortEnum, GeneralReleaseEntity>? _releases;
        private readonly SemaphoreSlim _semaphore = new(1);

        public PortsReleasesProvider(ApiInterface apiInterface)
        {
            _apiInterface = apiInterface;
        }

        /// <summary>
        /// Get the latest release of the selected port
        /// </summary>
        /// <param name="port">Port</param>
        public async Task<GeneralReleaseEntity?> GetLatestReleaseAsync(PortEnum portEnum)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);

            if (_releases is null)
            {
                _releases = await GetReleasesAsync().ConfigureAwait(false);
            }

            _semaphore.Release();

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
}
