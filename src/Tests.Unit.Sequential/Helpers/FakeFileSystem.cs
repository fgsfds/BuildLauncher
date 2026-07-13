using System.Text;
using Core.Client.Interfaces;

namespace Tests.Unit.Helpers;

public sealed class FakeFileSystem : IFileSystem
{
    private readonly Dictionary<string, byte[]> _files = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _directories = new(StringComparer.OrdinalIgnoreCase);

    public bool FileExists(string? path)
    {
        if (path is null)
            return false;

        return _files.ContainsKey(Normalize(path));
    }

    public bool DirectoryExists(string? path)
    {
        if (path is null)
            return false;

        var normalized = Normalize(path).TrimEnd('\\', '/');
        return _directories.Contains(normalized);
    }

    public void CreateDirectory(string path)
    {
        _directories.Add(Normalize(path).TrimEnd('\\', '/'));
    }

    public void DeleteFile(string path)
    {
        _files.Remove(Normalize(path));
    }

    public void DeleteDirectory(string path, bool recursive)
    {
        var normalized = Normalize(path).TrimEnd('\\', '/');
        _directories.Remove(normalized);

        if (recursive)
        {
            var prefix = normalized + '\\';
            foreach (var key in _files.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList())
            {
                _files.Remove(key);
            }

            foreach (var dir in _directories.Where(d => d.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList())
            {
                _directories.Remove(dir);
            }
        }
    }

    public void MoveFile(string source, string destination, bool overwrite = false)
    {
        var normalizedSource = Normalize(source);
        var normalizedDest = Normalize(destination);

        if (FileExists(destination) && overwrite)
        {
            _files.Remove(normalizedDest);
        }

        if (_files.TryGetValue(normalizedSource, out var data))
        {
            _files.Remove(normalizedSource);
            _files[normalizedDest] = data;
        }
    }

    public void MoveDirectory(string source, string destination)
    {
        var normSource = Normalize(source).TrimEnd('\\', '/');
        var normDest = Normalize(destination).TrimEnd('\\', '/');

        if (_directories.Remove(normSource))
        {
            _directories.Add(normDest);
        }

        foreach (var key in _files.Keys.Where(k => k.StartsWith(normSource + '\\', StringComparison.OrdinalIgnoreCase)).ToList())
        {
            var relative = key[(normSource.Length)..];
            var newKey = normDest + relative;
            _files[newKey] = _files[key];
            _files.Remove(key);
        }
    }

    public string[] GetFiles(string path)
    {
        var normalized = Normalize(path).TrimEnd('\\', '/') + '\\';
        return _files.Keys
            .Where(k => k.StartsWith(normalized, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public string[] GetFiles(string path, string searchPattern)
    {
        return GetFiles(path);
    }

    public void WriteAllText(string path, string contents)
    {
        _files[Normalize(path)] = Encoding.UTF8.GetBytes(contents);
    }

    public string ReadAllText(string path)
    {
        return Encoding.UTF8.GetString(_files[Normalize(path)]);
    }

    public string[] ReadAllLines(string path)
    {
        return ReadAllText(path).Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
    }

    public Stream OpenRead(string path)
    {
        return new MemoryStream(_files[Normalize(path)]);
    }

    public Stream CreateFile(string path)
    {
        _files[Normalize(path)] = [];
        return new FakeFileStream(this, Normalize(path));
    }

    public FileInfo? GetFileInfo(string path)
    {
        if (!FileExists(path))
            return null;

        return new FileInfo(path);
    }

    public void AddFile(string path, byte[] data)
    {
        _files[Normalize(path)] = data;

        var dir = Path.GetDirectoryName(path);
        while (dir is not null)
        {
            _directories.Add(Normalize(dir).TrimEnd('\\', '/'));
            dir = Path.GetDirectoryName(dir);
        }
    }

    public void AddFile(string path, string contents) => AddFile(path, Encoding.UTF8.GetBytes(contents));

    private static string Normalize(string path) => path.Replace('/', '\\');


    private sealed class FakeFileStream : MemoryStream
    {
        private readonly FakeFileSystem _fs;
        private readonly string _path;

        public FakeFileStream(FakeFileSystem fs, string path)
        {
            _fs = fs;
            _path = path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fs._files[_path] = ToArray();
            }

            base.Dispose(disposing);
        }
    }
}
