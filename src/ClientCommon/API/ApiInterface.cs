using ClientCommon.Config;
using Common.Entities;
using Common.Enums;
using Common.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClientCommon.API
{
    public sealed class ApiInterface
    {
        private readonly HttpClient _httpClient;
        private readonly ConfigEntity _config;

        private string ApiUrl => _config.UseLocalApi ? "https://localhost:7093/api" : "https://buildlauncher.fgsfds.link/api";

        public ApiInterface(
            ConfigProvider configProvider,
            HttpClient httpClient
            )
        {
            _config = configProvider.Config;
            _httpClient = httpClient;
        }

        public async Task<GeneralReleaseEntity?> GetLatestAppReleaseAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{ApiUrl}/releases/app").ConfigureAwait(false);

                if (response is null || string.IsNullOrWhiteSpace(response))
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

                if (response is null)
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

                if (response is null)
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

        public async Task<List<DownloadableAddonEntity>?> GetAddons(GameEnum gameEnum)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{ApiUrl}/addons/{gameEnum}").ConfigureAwait(false);

                if (response is null)
                {
                    return null;
                }

                var addons = JsonSerializer.Deserialize(response, DownloadableAddonEntityListContext.Default.ListDownloadableAddonEntity);

                return addons;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<int?> ChangeVoteAsync(IAddon addon, sbyte increment)
        {
            try
            {
                using var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/addons/scores/change", new Tuple<string, sbyte>(addon.Id, increment)).ConfigureAwait(false);
                var responseStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (responseStr is null)
                {
                    return null;
                }

                var newScore = int.TryParse(responseStr, out var newScoreInt);

                return newScore ? newScoreInt : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Dictionary<string, int>?> GetScores()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{ApiUrl}/addons/scores").ConfigureAwait(false);

                if (response is null)
                {
                    return null;
                }

                var addons = JsonSerializer.Deserialize<Dictionary<string, int>>(response);

                return addons;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> IncreaseNumberOfInstalls(string addonId)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/addons/installs/add", addonId).ConfigureAwait(false);

                if (response is null || !response.IsSuccessStatusCode)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
