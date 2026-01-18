using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace Common.Client.Tools;

public sealed class FilesDownloader
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    /// <summary>
    /// Operation progress
    /// </summary>
    public event EventHandler<float>? ProgressChanged;

    public FilesDownloader(
        IHttpClientFactory httpClientFactory,
        ILogger logger
        )
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Download archive
    /// </summary>
    /// <param name="url">Link to file download</param>
    /// <param name="filePath">Absolute path to destination file</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <exception cref="Exception">Error while downloading file</exception>
    public async Task<bool> DownloadFileAsync(
        Uri url,
        string filePath,
        CancellationToken cancellationToken
        )
    {
        _logger.LogInformation($"Starting file downloading: {url}");

        var tempFile = filePath + ".temp";

        if (File.Exists(tempFile))
        {
            File.Delete(tempFile);
        }

        using var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error while downloading {url}, error: {response.StatusCode}");
        }

        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var contentLength = response.Content.Headers.ContentLength;

        _logger.LogInformation($"File length is {contentLength}");

        FileStream fileStream = new(tempFile, FileMode.Create, FileAccess.Write, FileShare.None);

        try
        {
            if (!contentLength.HasValue)
            {
                await source.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var buffer = new byte[81920];
                var totalBytesRead = 0f;
                int bytesRead;

                while ((bytesRead = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
                    totalBytesRead += bytesRead;
                    ProgressChanged?.Invoke(this, totalBytesRead / (long)contentLength * 100);
                }
            }
        }
        catch (HttpIOException)
        {
            await ContinueDownload(url, contentLength, fileStream!, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            if (fileStream is not null)
            {
                await fileStream.DisposeAsync().ConfigureAwait(false);
            }

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            return false;
        }
        finally
        {
            await fileStream.DisposeAsync().ConfigureAwait(false);
        }

        _logger.LogInformation("Downloading finished, renaming temp file");
        File.Move(tempFile, filePath, true);
        return true;
    }


    /// <summary>
    /// Continue download after network error
    /// </summary>
    /// <param name="url">Url to the file</param>
    /// <param name="contentLength">Total content length</param>
    /// <param name="fileStream">File stream to write to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Task ContinueDownload(
        Uri url,
        long? contentLength,
        FileStream fileStream,
        CancellationToken cancellationToken
        )
    {
        _logger.LogInformation("Trying to continue downloading after failing");

        try
        {
            using HttpRequestMessage request = new()
            {
                RequestUri = url,
                Method = HttpMethod.Get
            };

            request.Headers.Range = new RangeHeaderValue(fileStream.Position, contentLength);

            using var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode is not System.Net.HttpStatusCode.PartialContent)
            {
                throw new InvalidOperationException("Error while downloading a file: " + response.StatusCode);
            }

            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            await source.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpIOException)
        {
            await ContinueDownload(url, contentLength, fileStream, cancellationToken).ConfigureAwait(false);
        }
    }
}
