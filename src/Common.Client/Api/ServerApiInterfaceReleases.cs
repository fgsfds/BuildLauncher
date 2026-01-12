using System.Net.Http.Json;
using Api.Common.Requests;
using Api.Common.Responses;
using Common.All.Enums;
using Common.All.Helpers;
using Common.All.Serializable.Downloadable;
using Common.Client.Helpers;

namespace Common.Client.Api;

public sealed partial class ServerApiInterface
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

            using var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();

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

    public Task<GeneralReleaseJsonModel?> GetLatestPortReleaseAsync(PortEnum portEnum) => Task.FromResult<GeneralReleaseJsonModel?>(null);

    public Task<GeneralReleaseJsonModel?> GetLatestToolReleaseAsync(ToolEnum toolEnum) => Task.FromResult<GeneralReleaseJsonModel?>(null);
}
