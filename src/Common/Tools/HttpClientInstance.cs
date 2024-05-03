namespace Common.Tools
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

        public async Task<string> GetStringAsync(string str)
        {
            await _semaphore.WaitAsync();

            var result = await _httpClient.GetStringAsync(str).ConfigureAwait(false);

            _semaphore.Release();

            return result;
        }

        public async Task<string> GetStringAsync(Uri str)
        {
            await _semaphore.WaitAsync();

            var result = await _httpClient.GetStringAsync(str).ConfigureAwait(false);

            _semaphore.Release();

            return result;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
