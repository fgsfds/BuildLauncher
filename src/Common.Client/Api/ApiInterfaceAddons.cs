using Api.Common.Requests;
using Api.Common.Responses;
using Common.Client.Helpers;
using Common.Client.Interfaces;
using Common.Entities;
using Common.Enums;
using System.Net.Http.Json;
using System.Web;

namespace Common.Client.Api;

public sealed partial class ApiInterface : IApiInterface
{
    private readonly HttpClient _httpClient;
    private readonly IConfigProvider _config;

    private string ApiUrl => _config.UseLocalApi ? "https://localhost:7126/api" : "https://buildlauncher.fgsfds.link/api";

    public ApiInterface(
        IConfigProvider config,
        HttpClient httpClient
        )
    {
        _config = config;
        _httpClient = httpClient;
    }

    public async Task<List<DownloadableAddonEntity>?> GetAddonsAsync(GameEnum gameEnum)
    {
        try
        {
            GetAddonsRequest message = new()
            {
                GameEnum = gameEnum,
                //ClientVersion = ClientProperties.CurrentVersion
            };

            using HttpRequestMessage requestMessage = new(HttpMethod.Get, $"{ApiUrl}/addons");
            requestMessage.Content = JsonContent.Create(message);

            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

            if (response is null || !response.IsSuccessStatusCode)
            {
                return null;
            }

            var addons = await response.Content.ReadFromJsonAsync<GetAddonsResponse>().ConfigureAwait(false);

            if (addons is null)
            {
                return null;
            }

            return addons.AddonsList;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<Dictionary<string, decimal>?> GetRatingsAsync()
    {
        try
        {
            GetRatingsRequest message = new()
            {
                ClientVersion = ClientProperties.CurrentVersion
            };

            using HttpRequestMessage requestMessage = new(HttpMethod.Get, $"{ApiUrl}/addons/ratings");
            requestMessage.Content = JsonContent.Create(message);

            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

            if (response is null || !response.IsSuccessStatusCode)
            {
                return null;
            }

            var ratings = await response.Content.ReadFromJsonAsync<GetRatingsResponse>().ConfigureAwait(false);

            if (ratings is null)
            {
                return null;
            }

            return ratings.Ratings;
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

            if (string.IsNullOrWhiteSpace(responseStr))
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

    public async Task<bool> IncreaseNumberOfInstallsAsync(string addonId)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/addons/installs/add", addonId).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
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

            if (!response.IsSuccessStatusCode)
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
