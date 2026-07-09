using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace Core.Client.Tools;

/// <summary>
///     Provides file downloading functionality with resume support and progress reporting.
/// </summary>
public sealed class FilesDownloader
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly ILogger<FilesDownloader> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FilesDownloader" /> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">Logger instance.</param>
    public FilesDownloader(
        IHttpClientFactory httpClientFactory,
        ILogger<FilesDownloader> logger
        )
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    ///     Operation progress
    /// </summary>
    public event EventHandler<float>? ProgressChanged;

    /// <summary>
    ///     Download archive
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
            throw new HttpRequestException($"Error while downloading {url}, error: {response.StatusCode}");
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
            await ContinueDownload(url, contentLength, fileStream, cancellationToken).ConfigureAwait(false);
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
    ///     Continues a failed download by issuing a ranged HTTP request.
    /// </summary>
    /// <param name="url">The file download URL.</param>
    /// <param name="contentLength">The total content length of the file.</param>
    /// <param name="fileStream">The file stream to append data to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    private async Task ContinueDownload(
        Uri url,
        long? contentLength,
        FileStream fileStream,
        CancellationToken cancellationToken,
        int retryCount = 0
        )
    {
        const int maxRetries = 3;

        _logger.LogInformation("Trying to continue downloading after failing (attempt {RetryCount}/{MaxRetries})", retryCount + 1, maxRetries);

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

            if (response.StatusCode is not HttpStatusCode.PartialContent)
            {
                throw new InvalidOperationException("Error while downloading a file: " + response.StatusCode);
            }

            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            await source.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpIOException) when (retryCount < maxRetries)
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
            _logger.LogWarning("HttpIOException during download. Retrying in {Delay} seconds...", delay.TotalSeconds);
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            await ContinueDownload(url, contentLength, fileStream, cancellationToken, retryCount + 1).ConfigureAwait(false);
        }
        catch (HttpIOException)
        {
            _logger.LogError("Failed to continue download after {MaxRetries} retries.", maxRetries);

            throw;
        }
    }
}
