using System.Runtime.CompilerServices;
using Core.All;

namespace Core.Client.Interfaces;

public interface IFilesUploader
{
    Task<Result<Uri?>> UploadFileAsync(string pathToLocalFile, string relativePathToRemoteFile, StrongBox<int> progress, CancellationToken cancellationToken);
}