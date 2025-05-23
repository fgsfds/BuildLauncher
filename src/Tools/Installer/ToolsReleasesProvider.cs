using Common.Client.Interfaces;
using Common.Common.Serializable.Downloadable;
using Common.Enums;
using Tools.Tools;

namespace Tools.Installer;

/// <summary>
/// Class that provides releases from tools' repositories
/// </summary>
public sealed class ToolsReleasesProvider
{
    private readonly IApiInterface _apiInterface;

    private Dictionary<ToolEnum, GeneralReleaseJsonModel>? _releases;
    private readonly SemaphoreSlim _semaphore = new(1);

    public ToolsReleasesProvider(IApiInterface apiInterface)
    {
        _apiInterface = apiInterface;
    }

    /// <summary>
    /// Get the latest release of the selected tool
    /// </summary>
    /// <param name="tool">Tool</param>
    public async Task<GeneralReleaseJsonModel?> GetLatestReleaseAsync(BaseTool tool)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        _releases ??= await GetReleasesAsync().ConfigureAwait(false);

        _ = _semaphore.Release();

        if (_releases is null)
        {
            return null;
        }

        var hasRelease = _releases.TryGetValue(tool.ToolEnum, out var release);

        return hasRelease ? release : null;
    }

    public async Task<Dictionary<ToolEnum, GeneralReleaseJsonModel>?> GetReleasesAsync()
    {
        return await _apiInterface.GetLatestToolsReleasesAsync().ConfigureAwait(false);
    }
}
