using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Amazon.S3;
using Core.All;
using Core.Client.Interfaces;
using Microsoft.Extensions.Logging;

namespace S3;

public sealed class S3FilesUploader : IFilesUploader
{
    private readonly S3UtilitiesFactory _s3Factory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<S3FilesUploader> _logger;

    public S3FilesUploader(S3UtilitiesFactory s3Factory, IHttpClientFactory httpClientFactory, ILogger<S3FilesUploader> logger)
    {
        _s3Factory = s3Factory;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<Result<Uri?>> UploadFileAsync(string pathToFile, string s3FileKey, StrongBox<int> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Uploading file to S3.");

        try
        {
            using var fileStream = File.OpenRead(pathToFile);
            _ = Task.Run(() => TrackProgress(fileStream, progress), cancellationToken);

            var sha = await SHA256.HashDataAsync(fileStream, cancellationToken).ConfigureAwait(false);
            var shaStr = Convert.ToHexString(sha);
            fileStream.Position = 0;

            var fileKey = S3Constants.S3SubFolder + "/" + s3FileKey;
            using var transferUtility = _s3Factory.CreateTransferUtility();
            _ = await transferUtility.UploadAsync(fileStream, fileKey, shaStr, cancellationToken).ConfigureAwait(false);

            Uri fullPath = new($"{S3Constants.S3Endpoint}/{S3Constants.S3Bucket}/{fileKey}");
            var fileAvailability = await CheckFileAvailabilityAsync(fullPath).ConfigureAwait(false);

            if (!fileAvailability.IsSuccess)
            {
                return new(ResultEnum.Error, null, string.Empty);
            }

            return new(ResultEnum.Success, fullPath, string.Empty);
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Uploading cancelled.");
            return new(ResultEnum.Error, null, "Uploading cancelled.");
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 access error while uploading file.");
            return new(ResultEnum.Error, null, "Wrong secret key.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error while uploading file.");
            return new(ResultEnum.Error, null, ex.Message);
        }
    }

    private async Task<Result> CheckFileAvailabilityAsync(Uri downloadUrl)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

        return response.IsSuccessStatusCode
            ? new(ResultEnum.Success, string.Empty)
            : new(ResultEnum.Error, $"File {downloadUrl} doesn't exist.");
    }

    private static void TrackProgress(FileStream streamToTrack, StrongBox<int> progress)
    {
        while (streamToTrack.CanSeek)
        {
            var pos = streamToTrack.Position / (float)streamToTrack.Length * 100;
            progress.Value = (int)pos;
            Thread.Sleep(50);
        }
    }
}