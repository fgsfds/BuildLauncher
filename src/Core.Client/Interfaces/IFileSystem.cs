namespace Core.Client.Interfaces;

/// <summary>
///     Abstracts file system operations to allow substitution in tests.
/// </summary>
public interface IFileSystem
{
    /// <summary>
    ///     Determines whether the specified file exists.
    /// </summary>
    /// <param name="path">
    ///     The file path.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if the file exists; otherwise
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    bool FileExists(string? path);

    /// <summary>
    ///     Determines whether the specified directory exists.
    /// </summary>
    /// <param name="path">
    ///     The directory path.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if the directory exists; otherwise
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    bool DirectoryExists(string? path);

    /// <summary>
    ///     Creates the specified directory.
    /// </summary>
    /// <param name="path">
    ///     The directory path.
    /// </param>
    void CreateDirectory(string path);

    /// <summary>
    ///     Deletes the specified file.
    /// </summary>
    /// <param name="path">
    ///     The file path.
    /// </param>
    void DeleteFile(string path);

    /// <summary>
    ///     Deletes the specified directory.
    /// </summary>
    /// <param name="path">
    ///     The directory path.
    /// </param>
    /// <param name="recursive">
    ///     Whether to delete the directory and its contents recursively.
    /// </param>
    void DeleteDirectory(string path, bool recursive);

    /// <summary>
    ///     Moves a file to a new location.
    /// </summary>
    /// <param name="source">
    ///     The source file path.
    /// </param>
    /// <param name="destination">
    ///     The destination file path.
    /// </param>
    /// <param name="overwrite">
    ///     Whether to overwrite an existing destination file.
    /// </param>
    void MoveFile(string source, string destination, bool overwrite = false);

    /// <summary>
    ///     Moves a directory to a new location.
    /// </summary>
    /// <param name="source">
    ///     The source directory path.
    /// </param>
    /// <param name="destination">
    ///     The destination directory path.
    /// </param>
    void MoveDirectory(string source, string destination);

    /// <summary>
    ///     Gets the files in the specified directory.
    /// </summary>
    /// <param name="path">
    ///     The directory path.
    /// </param>
    /// <returns>
    ///     The file paths.
    /// </returns>
    IReadOnlyList<string> GetFiles(string path);

    /// <summary>
    ///     Gets the files in the specified directory that match the search pattern.
    /// </summary>
    /// <param name="path">
    ///     The directory path.
    /// </param>
    /// <param name="searchPattern">
    ///     The search pattern.
    /// </param>
    /// <returns>
    ///     The matching file paths.
    /// </returns>
    IReadOnlyList<string> GetFiles(string path, string searchPattern);

    /// <summary>
    ///     Writes text to the specified file.
    /// </summary>
    /// <param name="path">
    ///     The file path.
    /// </param>
    /// <param name="contents">
    ///     The text to write.
    /// </param>
    void WriteAllText(string path, string contents);

    /// <summary>
    ///     Reads all text from the specified file.
    /// </summary>
    /// <param name="path">
    ///     The file path.
    /// </param>
    /// <returns>
    ///     The file contents.
    /// </returns>
    string ReadAllText(string path);

    /// <summary>
    ///     Reads all lines from the specified file.
    /// </summary>
    /// <param name="path">
    ///     The file path.
    /// </param>
    /// <returns>
    ///     The file lines.
    /// </returns>
    IReadOnlyList<string> ReadAllLines(string path);

    /// <summary>
    ///     Opens the specified file for reading.
    /// </summary>
    /// <param name="path">
    ///     The file path.
    /// </param>
    /// <returns>
    ///     A readable stream.
    /// </returns>
    Stream OpenRead(string path);

    /// <summary>
    ///     Creates or overwrites the specified file.
    /// </summary>
    /// <param name="path">
    ///     The file path.
    /// </param>
    /// <returns>
    ///     A writable stream.
    /// </returns>
    Stream CreateFile(string path);

    /// <summary>
    ///     Gets information about the specified file.
    /// </summary>
    /// <param name="path">
    ///     The file path.
    /// </param>
    /// <returns>
    ///     The file information, or
    ///     <c>
    ///         null
    ///     </c>
    ///     if the file does not exist.
    /// </returns>
    FileInfo? GetFileInfo(string path);
}
