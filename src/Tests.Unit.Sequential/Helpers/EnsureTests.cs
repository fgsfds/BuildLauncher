using Core.Client.Helpers;

namespace Tests.Unit.Helpers;

public sealed class EnsureTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [Fact]
    public void DirectoryExists_CreatesDirectory()
    {
        var path = Path.Combine(_tempDir, "NewFolder");

        Assert.False(Directory.Exists(path));

        Ensure.DirectoryExists(path);

        Assert.True(Directory.Exists(path));
    }

    [Fact]
    public void DirectoryExists_WhenExists_DoesNotThrow()
    {
        Directory.CreateDirectory(_tempDir);

        Ensure.DirectoryExists(_tempDir);

        Assert.True(Directory.Exists(_tempDir));
    }

    [Fact]
    public void DirectoryExists_CreatesParentDirectories()
    {
        var path = Path.Combine(_tempDir, "parent", "child");

        Ensure.DirectoryExists(path);

        Assert.True(Directory.Exists(path));
    }

    [Fact]
    public void DirectoryExists_WhenExists_DoesNotDeleteExistingContent()
    {
        var contentPath = Path.Combine(_tempDir, "subfolder");
        Directory.CreateDirectory(contentPath);
        var filePath = Path.Combine(contentPath, "test.txt");
        File.WriteAllText(filePath, "content");

        Ensure.DirectoryExists(contentPath);

        Assert.True(File.Exists(filePath));
    }
}
