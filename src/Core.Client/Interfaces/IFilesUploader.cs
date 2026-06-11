using System.Runtime.CompilerServices;
using Core.All;
using Core.Client.Helpers;

namespace Core.Client.Interfaces;

/// <summary>
/// Defines the interface for uploading files.
/// </summary>
public interface IFilesUploader
{
    /// <summary>
    /// Upload file.
    /// </summary>
    /// <param name="pathToLocalFile">Absolute path to the local file.</param>
    /// <param name="relativePathToRemoteFile">Relative path to the remote file.</param>
    /// <param name="progress">Progress.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Resulting remote file url.</returns>
    Task<Result<RemoteFileMetadata?>> UploadFileAsync(string pathToLocalFile, string relativePathToRemoteFile, StrongBox<int> progress, CancellationToken cancellationToken);

    /// <summary>
    /// Upload file to public uploads folder.
    /// </summary>
    /// <param name="pathToLocalFile">Absolute path to the local file.</param>
    /// <param name="relativePathToRemoteFile">Relative path to the remote file.</param>
    /// <param name="progress">Progress.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Resulting remote file url.</returns>
    Task<Result<RemoteFileMetadata?>> UploadFileToPublicAsync(string pathToLocalFile, string relativePathToRemoteFile, StrongBox<int> progress, CancellationToken cancellationToken);
}
