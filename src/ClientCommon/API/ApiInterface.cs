﻿using ClientCommon.Config;
using Common.Entities;
using Common.Enums;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace ClientCommon.API
{
    public sealed class ApiInterface
    {
        private readonly HttpClient _httpClient;
        private readonly IConfigProvider _config;

        private string ApiUrl => _config.UseLocalApi ? "https://localhost:7093/api" : "https://buildlauncher.fgsfds.link/api";

        public ApiInterface(
            IConfigProvider config,
            HttpClient httpClient
            )
        {
            _config = config;
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

        public async Task<List<DownloadableAddonEntity>?> GetAddonsAsync(GameEnum gameEnum)
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

        public async Task<decimal?> ChangeScoreAsync(string addonId, sbyte score, bool isNew)
        {
            try
            {
                using var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/addons/rating/change", new Tuple<string, sbyte, bool>(addonId, score, isNew)).ConfigureAwait(false);
                var responseStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (responseStr is null)
                {
                    return null;
                }

                var newScore = decimal.TryParse(responseStr, out var newScoreInt);

                return newScore ? newScoreInt : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Dictionary<string, decimal>?> GetScoresAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{ApiUrl}/addons/rating").ConfigureAwait(false);

                if (response is null)
                {
                    return null;
                }

                var addons = JsonSerializer.Deserialize<Dictionary<string, decimal>>(response);

                return addons;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> IncreaseNumberOfInstallsAsync(string addonId)
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

        public async Task<bool> AddAddonToDatabaseAsync(AddonsJsonEntity addon)
        {
            try
            {
                var apiPassword = _config.ApiPassword;

                var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/addons/add", new Tuple<AddonsJsonEntity, string>(addon, apiPassword)).ConfigureAwait(false);

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

        public async Task<string?> GetSignedUrlAsync(string path)
        {
            try
            {
                var encodedPath = HttpUtility.UrlEncode(path);

                var signedUrl = await _httpClient.GetStringAsync($"{ApiUrl}/storage/url/{encodedPath}").ConfigureAwait(false);

                return signedUrl;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
