using Common.Entities;
using Common.Enums;
using System.Text.Json;

namespace Common.API
{
    public sealed class ApiInterface
    {
        private readonly HttpClient _httpClient;

        public ApiInterface(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GeneralReleaseEntity?> GetLatestAppReleaseAsync()
        {
            var response = await _httpClient.GetStringAsync($"https://localhost:7093/api/releases/app").ConfigureAwait(false);

            if (response is null || string.IsNullOrWhiteSpace(response)) 
            {
                return null;
            }

            var release = JsonSerializer.Deserialize<GeneralReleaseEntity>(response);

            return release;
        }

        public async Task<Dictionary<PortEnum, GeneralReleaseEntity>?> GetLatestPortsReleasesAsync()
        {
            var response = await _httpClient.GetStringAsync($"https://localhost:7093/api/releases/ports").ConfigureAwait(false);

            if (response is null) 
            {
                return null;
            }

            var releases = JsonSerializer.Deserialize<Dictionary<PortEnum, GeneralReleaseEntity>>(response);

            return releases;
        }

        public async Task<Dictionary<ToolEnum, GeneralReleaseEntity>?> GetLatestToolsReleasesAsync()
        {
            var response = await _httpClient.GetStringAsync($"https://localhost:7093/api/releases/tools").ConfigureAwait(false);

            if (response is null) 
            {
                return null;
            }

            var releases = JsonSerializer.Deserialize<Dictionary<ToolEnum, GeneralReleaseEntity>>(response);

            return releases;
        }
    }
}
