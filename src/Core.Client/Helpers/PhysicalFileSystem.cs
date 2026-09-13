using Core.Client.Interfaces;

namespace Core.Client.Helpers;

/// <summary>
///     Default <see cref="IFileSystem" /> implementation backed by <see cref="System.IO" />.
/// </summary>
public sealed class PhysicalFileSystem : IFileSystem
{
    /// <summary>
    ///     The shared instance of the physical file system.
    /// </summary>
    public static readonly IFileSystem Instance = new PhysicalFileSystem();

    /// <inheritdoc />
    public bool FileExists(string? path) => File.Exists(path);

    /// <inheritdoc />
    public bool DirectoryExists(string? path) => Directory.Exists(path);

    /// <inheritdoc />
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    /// <inheritdoc />
    public void DeleteFile(string path) => File.Delete(path);

    /// <inheritdoc />
    public void DeleteDirectory(string path, bool recursive) => Directory.Delete(path, recursive);

    /// <inheritdoc />
    public void MoveFile(string source, string destination, bool overwrite = false)
    {
        if (File.Exists(destination) && overwrite)
        {
            File.Delete(destination);
        }

        File.Move(source, destination);
    }

    /// <inheritdoc />
    public void MoveDirectory(string source, string destination) => Directory.Move(source, destination);

    /// <inheritdoc />
    public IReadOnlyList<string> GetFiles(string path) => Directory.GetFiles(path);

    /// <inheritdoc />
    public IReadOnlyList<string> GetFiles(string path, string searchPattern) => Directory.GetFiles(path, searchPattern);

    /// <inheritdoc />
    public void WriteAllText(string path, string contents) => File.WriteAllText(path, contents);

    /// <inheritdoc />
    public string ReadAllText(string path) => File.ReadAllText(path);

    /// <inheritdoc />
    public IReadOnlyList<string> ReadAllLines(string path) => File.ReadAllLines(path);

    /// <inheritdoc />
    public Stream OpenRead(string path) => File.OpenRead(path);

    /// <inheritdoc />
    public Stream CreateFile(string path) => File.Create(path);

    /// <inheritdoc />
    public FileInfo? GetFileInfo(string path) => File.Exists(path) ? new FileInfo(path) : null;
}
