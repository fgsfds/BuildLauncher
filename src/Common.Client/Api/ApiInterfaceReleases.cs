using Api.Common.Requests;
using Api.Common.Responses;
using Common.Client.Helpers;
using Common.Common.Helpers;
using Common.Entities;
using Common.Enums;
using System.Net.Http.Json;

namespace Common.Client.Api;

public sealed partial class ApiInterface
{
    public async Task<GeneralReleaseEntity?> GetLatestAppReleaseAsync()
    {
        try
        {
            GetAppReleaseRequest message = new()
            {
                OSEnum = CommonProperties.OSEnum,
                ClientVersion = ClientProperties.CurrentVersion
            };

            using HttpRequestMessage requestMessage = new(HttpMethod.Get, $"{ApiUrl}/releases/app");
            requestMessage.Content = JsonContent.Create(message);

            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

            if (response?.IsSuccessStatusCode is not true)
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
            GetPortsReleasesRequest message = new()
            {
                OSEnum = CommonProperties.OSEnum,
                ClientVersion = ClientProperties.CurrentVersion
            };

            using HttpRequestMessage requestMessage = new(HttpMethod.Get, $"{ApiUrl}/releases/ports");
            requestMessage.Content = JsonContent.Create(message);

            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

            if (response?.IsSuccessStatusCode is not true)
            {
                return null;
            }

            var update = await response.Content.ReadFromJsonAsync<GetPortsReleasesResponse>().ConfigureAwait(false);

            if (update is null)
            {
                return null;
            }

            return update.PortsReleases;
        }
        catch
        {
            return null;
        }
    }

    public Task<Dictionary<ToolEnum, GeneralReleaseEntity>?> GetLatestToolsReleasesAsync() => null!;
}
