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

    /// <summary>
    ///     Initializes a new instance of the <see cref="S3FilesUploader" /> class.
    /// </summary>
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

    /// <summary>
    ///     Uploads a file to the public uploads folder on S3.
    /// </summary>
    /// <param name="pathToLocalFile">Path to the local file.</param>
    /// <param name="relativePathToRemoteFile">Relative path in the remote storage.</param>
    /// <param name="progress">Progress indicator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The upload result with remote file metadata.</returns>
    public Task<Result<RemoteFileMetadata?>> UploadFileToPublicAsync(
        string pathToLocalFile,
        string relativePathToRemoteFile,
        StrongBox<int> progress,
        CancellationToken cancellationToken)
    {
        var fileKey = "uploads/" + S3Constants.S3SubFolder + "/" + relativePathToRemoteFile;

        return InternalUploadAsync(pathToLocalFile, fileKey, progress, cancellationToken);
    }

    /// <summary>
    ///     Performs the internal file upload to S3.
    /// </summary>
    /// <param name="pathToLocalFile">Path to the local file.</param>
    /// <param name="fileKey">S3 object key.</param>
    /// <param name="progress">Progress indicator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The upload result with remote file metadata.</returns>
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
            CancellationTokenSource cts = new();

            await using var fileStream = File.OpenRead(pathToLocalFile);
            _ = Task.Run(() => TrackProgress(fileStream, progress, cts.Token), cancellationToken);

            var sha = await SHA256.HashDataAsync(fileStream, cancellationToken).ConfigureAwait(false);
            var shaStr = Convert.ToHexString(sha);
            fileStream.Position = 0;

            using var transferUtility = _s3Factory.CreateTransferUtility();
            _ = await transferUtility.UploadAsync(fileStream, fileKey, shaStr, cancellationToken).ConfigureAwait(false);

            await cts.CancelAsync();

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

    /// <summary>
    ///     Tracks the upload progress by reading the stream position.
    /// </summary>
    /// <param name="streamToTrack">The file stream to track.</param>
    /// <param name="progress">Progress indicator to update.</param>
    /// <param name="cancellationToken">Cancellation token,</param>
    private static void TrackProgress(
        FileStream streamToTrack,
        StrongBox<int> progress,
        CancellationToken cancellationToken
        )
    {
        try
        {
            if (!streamToTrack.CanSeek)
            {
                return;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                var pos = streamToTrack.Position / (float)streamToTrack.Length * 100;
                progress.Value = (int)pos;

                if (cancellationToken.WaitHandle.WaitOne(50))
                {
                    break;
                }
            }
        }
        catch (ObjectDisposedException)
        {
            // stream was disposed — tracking is no longer needed
        }
        catch (NotSupportedException)
        {
            // stream doesn't support seeking — progress won't work
        }
    }
}
