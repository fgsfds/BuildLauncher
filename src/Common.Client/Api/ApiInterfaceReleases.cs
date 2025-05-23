using Api.Common.Requests;
using Api.Common.Responses;
using Common.Client.Helpers;
using Common.Common.Serializable.Downloadable;
using Common.Enums;
using Common.Helpers;
using System.Net.Http.Json;

namespace Common.Client.Api;

public sealed partial class ApiInterface
{
    public async Task<GeneralReleaseJsonModel?> GetLatestAppReleaseAsync()
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

            using var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

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

    public async Task<Dictionary<PortEnum, GeneralReleaseJsonModel>?> GetLatestPortsReleasesAsync()
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

            using var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

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

    public Task<Dictionary<ToolEnum, GeneralReleaseJsonModel>?> GetLatestToolsReleasesAsync() => null!;
}
