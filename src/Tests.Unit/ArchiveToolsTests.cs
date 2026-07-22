using System.IO.Compression;
using Core.Client.Tools;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests.Unit;

public sealed class ArchiveToolsTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private bool _disposed;

    public ArchiveToolsTests()
    {
        _ = Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;

            try
            {
                Directory.Delete(_tempDir, true);
            }
            catch
            {
            }
        }
    }

    [Fact]
    public async Task UnpackArchiveAsync_CancelledBeforeStart_ThrowsTaskCanceledException()
    {
        var archivePath = CreateTestArchive();
        var extractPath = Path.Combine(_tempDir, "extract");
        var archiveTools = new ArchiveTools(NullLogger<ArchiveTools>.Instance);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            archiveTools.UnpackArchiveAsync(archivePath, extractPath, cts.Token));
    }

    [Fact]
    public async Task UnpackArchiveAsync_WithValidArchive_ExtractsFiles()
    {
        var archivePath = CreateTestArchive();
        var extractPath = Path.Combine(_tempDir, "extract");
        var archiveTools = new ArchiveTools(NullLogger<ArchiveTools>.Instance);

        await archiveTools.UnpackArchiveAsync(archivePath, extractPath, CancellationToken.None);

        Assert.True(File.Exists(Path.Combine(extractPath, "test.txt")));
        Assert.Equal("hello", await File.ReadAllTextAsync(Path.Combine(extractPath, "test.txt")));
    }

    private static string CreateTestArchive(string dir)
    {
        var archivePath = Path.Combine(dir, "test.zip");
        var tempFile = Path.Combine(dir, "test.txt");
        File.WriteAllText(tempFile, "hello");

        using var stream = new FileStream(archivePath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);
        archive.CreateEntryFromFile(tempFile, "test.txt");

        return archivePath;
    }

    private string CreateTestArchive() => CreateTestArchive(_tempDir);
}