using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Amazon.S3;
using Core.All;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Microsoft.Extensions.Logging;

namespace S3;

/// <summary>
///     Provides functionality to upload files to S3.
/// </summary>
public sealed class S3FilesUploader : IFilesUploader
{
    private readonly ILogger<S3FilesUploader> _logger;
    private readonly S3UtilitiesFactory _s3Factory;

    public S3FilesUploader(S3UtilitiesFactory s3Factory, ILogger<S3FilesUploader> logger)
    {
        _s3Factory = s3Factory;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<Result<RemoteFileMetadata?>> UploadFileAsync(
        string pathToLocalFile,
        string relativePathToRemoteFile,
        StrongBox<int> progress,
        CancellationToken cancellationToken
        )
    {
        var fileKey = S3Constants.S3SubFolder + "/" + relativePathToRemoteFile;

        return InternalUploadAsync(pathToLocalFile, fileKey, progress, cancellationToken);
    }

    public Task<Result<RemoteFileMetadata?>> UploadFileToPublicAsync(
        string pathToLocalFile,
        string relativePathToRemoteFile,
        StrongBox<int> progress,
        CancellationToken cancellationToken)
    {
        var fileKey = "uploads/" + S3Constants.S3SubFolder + "/" + relativePathToRemoteFile;

        return InternalUploadAsync(pathToLocalFile, fileKey, progress, cancellationToken);
    }

    private async Task<Result<RemoteFileMetadata?>> InternalUploadAsync(
        string pathToLocalFile,
        string fileKey,
        StrongBox<int> progress,
        CancellationToken cancellationToken
        )
    {
        _logger.LogInformation("Uploading file to S3.");

        try
        {
            using var fileStream = File.OpenRead(pathToLocalFile);
            _ = Task.Run(() => TrackProgress(fileStream, progress), cancellationToken);

            var sha = await SHA256.HashDataAsync(fileStream, cancellationToken).ConfigureAwait(false);
            var shaStr = Convert.ToHexString(sha);
            fileStream.Position = 0;

            using var transferUtility = _s3Factory.CreateTransferUtility();
            _ = await transferUtility.UploadAsync(fileStream, fileKey, shaStr, cancellationToken).ConfigureAwait(false);

            var metadataProvider = _s3Factory.CreateMetadataProvider();
            var fileMetadata = await metadataProvider.GetMetadata(fileKey).ConfigureAwait(false);

            if (fileMetadata.Size < 1)
            {
                return new(ResultEnum.Error, null, string.Empty);
            }

            return new(ResultEnum.Success, fileMetadata, string.Empty);
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
