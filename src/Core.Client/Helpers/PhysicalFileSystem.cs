using Core.Client.Interfaces;

namespace Core.Client.Helpers;

public sealed class PhysicalFileSystem : IFileSystem
{
    public static readonly IFileSystem Instance = new PhysicalFileSystem();

    public bool FileExists(string? path) => File.Exists(path);

    public bool DirectoryExists(string? path) => Directory.Exists(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public void DeleteFile(string path) => File.Delete(path);

    public void DeleteDirectory(string path, bool recursive) => Directory.Delete(path, recursive);

    public void MoveFile(string source, string destination, bool overwrite = false)
    {
        if (File.Exists(destination) && overwrite)
        {
            File.Delete(destination);
        }

        File.Move(source, destination);
    }

    public void MoveDirectory(string source, string destination) => Directory.Move(source, destination);

    public string[] GetFiles(string path) => Directory.GetFiles(path);

    public string[] GetFiles(string path, string searchPattern) => Directory.GetFiles(path, searchPattern);

    public void WriteAllText(string path, string contents) => File.WriteAllText(path, contents);

    public string ReadAllText(string path) => File.ReadAllText(path);

    public string[] ReadAllLines(string path) => File.ReadAllLines(path);

    public Stream OpenRead(string path) => File.OpenRead(path);

    public Stream CreateFile(string path) => File.Create(path);

    public FileInfo? GetFileInfo(string path) => File.Exists(path) ? new FileInfo(path) : null;
}
