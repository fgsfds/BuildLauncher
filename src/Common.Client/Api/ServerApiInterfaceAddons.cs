using System.Net.Http.Json;
using System.Web;
using Api.Common.Requests;
using Api.Common.Responses;
using Common.All.Enums;
using Common.All.Serializable.Downloadable;
using Common.Client.Helpers;
using Common.Client.Interfaces;

namespace Common.Client.Api;

public sealed partial class ServerApiInterface : IApiInterface
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfigProvider _config;

    private string ApiUrl => _config.UseLocalApi ? "https://localhost:7126/api" : "https://buildlauncher.fgsfds.link/api";

    public ServerApiInterface(
        IConfigProvider config,
        IHttpClientFactory httpClientFactory
        )
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<DownloadableAddonJsonModel>?> GetAddonsAsync(GameEnum gameEnum)
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

            using var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();

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

            using var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);

            if (response?.IsSuccessStatusCode is not true)
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
            using var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.PutAsJsonAsync($"{ApiUrl}/addons/rating/change", new Tuple<string, sbyte, bool>(addonId, score, isNew)).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            
            var responseStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);


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
            using var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.PutAsJsonAsync($"{ApiUrl}/addons/installs/add", addonId).ConfigureAwait(false);

            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> AddAddonToDatabaseAsync(DownloadableAddonJsonModel addon)
    {
        try
        {
            var apiPassword = _config.ApiPassword;

            using var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.PostAsJsonAsync($"{ApiUrl}/addons/add", new Tuple<DownloadableAddonJsonModel, string>(addon, apiPassword)).ConfigureAwait(false);

            return response.IsSuccessStatusCode;
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

            using var httpClient = _httpClientFactory.CreateClient();
            var signedUrl = await httpClient.GetStringAsync($"{ApiUrl}/storage/url/{encodedPath}").ConfigureAwait(false);

            return signedUrl;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public Task<string?> GetUploadFolderAsync() => throw new NotImplementedException();
}
