using Common.Entities;
using Common.Enums;
using System.Text.Json;

namespace Api.Common.Interface;

public sealed partial class ApiInterface
{
    public async Task<GeneralReleaseEntity?> GetLatestAppReleaseAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{ApiUrl}/releases/app").ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(response))
            {
                return null;
            }

            var release = JsonSerializer.Deserialize<GeneralReleaseEntity>(response);

            return release;
        }
        catch
        {
            return null;
        }
    }

    public async Task<Dictionary<PortEnum, GeneralReleaseEntity>?> GetLatestPortsReleasesAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{ApiUrl}/releases/ports").ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(response))
            {
                return null;
            }

            var releases = JsonSerializer.Deserialize<Dictionary<PortEnum, GeneralReleaseEntity>>(response);

            return releases;
        }
        catch
        {
            return null;
        }
    }

    public async Task<Dictionary<ToolEnum, GeneralReleaseEntity>?> GetLatestToolsReleasesAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{ApiUrl}/releases/tools").ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(response))
            {
                return null;
            }

            var releases = JsonSerializer.Deserialize<Dictionary<ToolEnum, GeneralReleaseEntity>>(response);

            return releases;
        }
        catch
        {
            return null;
        }
    }
}
