using CommunityToolkit.Diagnostics;
using Downloader;
using Microsoft.Extensions.Logging;

namespace Common.Client.Tools;

public sealed class FilesDownloader
{
    private readonly DownloadService _downloadService;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    /// <summary>
    /// Operation progress
    /// </summary>
    public event EventHandler<float>? ProgressChanged;

    public FilesDownloader(
        DownloadService downloadService,
        HttpClient httpClient,
        ILogger logger
        )
    {
        _downloadService = downloadService;
        _httpClient = httpClient;
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

        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            ThrowHelper.ThrowExternalException($"Error while downloading {url}, error: {response.StatusCode}");
        }

        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var contentLength = response.Content.Headers.ContentLength;

        _logger.LogInformation($"File length is {contentLength}");

        try
        {
            _downloadService.DownloadProgressChanged += OnDownloadProgressChangedEvent;
             await _downloadService.DownloadFileTaskAsync(url.ToString(), tempFile, cancellationToken).ConfigureAwait(false);

            if (_downloadService.Status is DownloadStatus.Stopped or DownloadStatus.Failed)
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "");
            return false;
        }

        _logger.LogInformation("Downloading finished, renaming temp file");
        File.Move(tempFile, filePath, true);
        return true;
    }

    private void OnDownloadProgressChangedEvent(object? sender, DownloadProgressChangedEventArgs e)
    {
        ProgressChanged?.Invoke(this, (float)e.ProgressPercentage);
    }
}
