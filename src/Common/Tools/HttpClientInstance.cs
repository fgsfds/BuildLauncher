﻿namespace Common.Tools
{
    public sealed class HttpClientInstance : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _semaphore = new(1);

        public HttpClientInstance()
        {
            _httpClient = new();
            _httpClient.Timeout = TimeSpan.FromMinutes(1);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("BuildLauncher");
        }

        /// <summary>
        /// Send a GET request to the specified Uri and return the response body as a string
        /// </summary>
        /// <param name="url">Uri</param>
        public async Task<string> GetStringAsync(string url)
        {
            await _semaphore.WaitAsync();

            var result = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

            _semaphore.Release();

            return result;
        }

        /// <summary>
        /// Send a GET request to the specified Uri and return the response body as a string
        /// </summary>
        /// <param name="url">Uri</param>
        public async Task<string> GetStringAsync(Uri url)
        {
            await _semaphore.WaitAsync();

            var result = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

            _semaphore.Release();

            return result;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
