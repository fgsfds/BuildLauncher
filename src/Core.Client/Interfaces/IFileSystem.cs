namespace Core.Client.Interfaces;

public interface IFileSystem
{
    bool FileExists(string? path);
    bool DirectoryExists(string? path);
    void CreateDirectory(string path);
    void DeleteFile(string path);
    void DeleteDirectory(string path, bool recursive);
    void MoveFile(string source, string destination, bool overwrite = false);
    void MoveDirectory(string source, string destination);
    string[] GetFiles(string path);
    string[] GetFiles(string path, string searchPattern);
    void WriteAllText(string path, string contents);
    string ReadAllText(string path);
    string[] ReadAllLines(string path);
    Stream OpenRead(string path);
    Stream CreateFile(string path);
    FileInfo? GetFileInfo(string path);
}
