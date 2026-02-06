using Microsoft.Extensions.Logging;
using SharpCompress.Archives;

namespace Common.Client.Tools;

public sealed class ArchiveTools
{
    private readonly ILogger _logger;

    /// <summary>
    /// Operation progress
    /// </summary>
    public event EventHandler<float>? ProgressChanged;

    public ArchiveTools(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Unpack archive
    /// </summary>
    /// <param name="pathToArchive">Absolute path to archive file</param>
    /// <param name="unpackTo">Directory to unpack archive to</param>
    public async Task UnpackArchiveAsync(
        string pathToArchive,
        string unpackTo
        )
    {
        var entryNumber = 1f;

        using var archive = ArchiveFactory.Open(pathToArchive);
        var count = archive.Entries.Count();

        foreach (var entry in archive.Entries)
        {
            var fileName = entry.Key!;

            if (string.IsNullOrWhiteSpace(fileName))
            {
                continue;
            }

            var fullName = Path.Combine(unpackTo, fileName);

            if (entry.IsDirectory)
            {
                _ = Directory.CreateDirectory(fullName);
            }
            else
            {
                var directory = Path.GetDirectoryName(fullName);

                if (directory is not null &&
                    !Directory.Exists(directory))
                {
                    _ = Directory.CreateDirectory(directory);
                }

                await entry.WriteToFileAsync(fullName).ConfigureAwait(false);
            }

            ProgressChanged?.Invoke(this, entryNumber / count * 100);

            entryNumber++;
        }
    }
}
