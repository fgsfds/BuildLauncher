using Common.API;
using Common.Entities;
using Common.Enums;
using Tools.Tools;

namespace Tools.Installer
{
    /// <summary>
    /// Class that provides releases from tools' repositories
    /// </summary>
    public partial class ToolsReleasesProvider
    {
        private readonly ApiInterface _apiInterface;

        private Dictionary<ToolEnum, GeneralReleaseEntity>? _releases;
        private readonly SemaphoreSlim _semaphore = new(1);

        public ToolsReleasesProvider(ApiInterface apiInterface)
        {
            _apiInterface = apiInterface;
        }

        /// <summary>
        /// Get the latest release of the selected tool
        /// </summary>
        /// <param name="tool">Tool</param>
        public async Task<GeneralReleaseEntity?> GetLatestReleaseAsync(BaseTool tool, bool forceCheck)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);

            if (_releases is null)
            {
                await GetReleasesAsync().ConfigureAwait(false);
            }

            _semaphore.Release();

            if (_releases is null)
            {
                return null;
            }

            var hasRelease = _releases.TryGetValue(tool.ToolEnum, out var release);

            return hasRelease ? release : null;
        }

        public async Task GetReleasesAsync()
        {
            _releases = await _apiInterface.GetLatestToolsReleasesAsync().ConfigureAwait(false);
        }
    }
}
