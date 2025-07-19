using System.Net.Http.Headers;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;

namespace Common.Client.Tools;

public sealed class ArchiveTools
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    /// <summary>
    /// Operation progress
    /// </summary>
    public readonly Progress<float> Progress = new();

    public ArchiveTools(
        HttpClient httpClient,
        ILogger logger
        )
    {
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

        IProgress<float> progress = Progress;
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
                    progress.Report(totalBytesRead / (long)contentLength * 100);
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
    /// Unpack archive
    /// </summary>
    /// <param name="pathToArchive">Absolute path to archive file</param>
    /// <param name="unpackTo">Directory to unpack archive to</param>
    /// <param name="isSubfolder">Unpack content from a subfolder inside the archive</param>
    public async Task UnpackArchiveAsync(
        string pathToArchive,
        string unpackTo,
        bool isSubfolder = false
        )
    {
        IProgress<float> progress = Progress;

        var entryNumber = 1f;

        await Task.Run(() =>
        {
            using var archive = ArchiveFactory.Open(pathToArchive);
            using var reader = archive.ExtractAllEntries();

            var count = archive.Entries.Count();

            while (reader.MoveToNextEntry())
            {
                var fileName = reader.Entry.Key!;

                if (isSubfolder)
                {
                    fileName = fileName[(fileName.IndexOf('/') + 1)..];
                }

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    continue;
                }

                var fullName = Path.Combine(unpackTo, fileName);

                if (reader.Entry.IsDirectory)
                {
                    _ = Directory.CreateDirectory(fullName);
                }
                else
                {
                    var directory = Path.GetDirectoryName(fullName);

                    if (directory is not null &&
                        !Directory.Exists(directory))
                    {
                        _ = Directory.CreateDirectory(directory);
                    }

                    using var writableStream = File.OpenWrite(fullName);
                    reader.WriteEntryTo(writableStream);
                }

                progress.Report(entryNumber / count * 100);

                entryNumber++;
            }
        }).ConfigureAwait(false);
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

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode is not System.Net.HttpStatusCode.PartialContent)
            {
                ThrowHelper.ThrowInvalidOperationException("Error while downloading a file: " + response.StatusCode);
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
