using Api.Common.Requests;
using Api.Common.Responses;
using Common.Entities;
using Common.Enums;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Client.Api;

public sealed partial class ApiInterface
{
    public async Task<GeneralReleaseEntity?> GetLatestAppReleaseAsync()
    {
        try
        {
            GetAppReleaseRequest message = new()
            {
                OSEnum = OSEnum.Windows
            };

            using HttpRequestMessage requestMessage = new(HttpMethod.Get, $"{ApiUrl}/releases/app");
            requestMessage.Content = JsonContent.Create(message);

            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

            if (response is null || !response.IsSuccessStatusCode)
            {
                return null;
            }

            var update = await response.Content.ReadFromJsonAsync<GetAppReleaseResponse>().ConfigureAwait(false);

            if (update is null)
            {
                return null;
            }

            return update.AppRelease;
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

            //var releases = JsonSerializer.Deserialize<Dictionary<ToolEnum, GeneralReleaseEntity>>(response);

            return [];
        }
        catch
        {
            return null;
        }
    }
}
