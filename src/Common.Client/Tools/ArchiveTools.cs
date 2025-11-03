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
    /// <param name="isSubfolder">Unpack content from a subfolder inside the archive</param>
    public async Task UnpackArchiveAsync(
        string pathToArchive,
        string unpackTo,
        bool isSubfolder = false
        )
    {
        var entryNumber = 1f;

        await Task.Run(() =>
        {
            using var archive = ArchiveFactory.Open(pathToArchive);
            using var reader = archive.ExtractAllEntries();

            var count = archive.Entries.Count();

            while (reader.MoveToNextEntry())
            {
                var fileName = reader.Entry.Key!;

                if (isSubfolder)
                {
                    fileName = fileName[(fileName.IndexOf('/') + 1)..];
                }

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    continue;
                }

                var fullName = Path.Combine(unpackTo, fileName);

                if (reader.Entry.IsDirectory)
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

                    using var writableStream = File.OpenWrite(fullName);
                    reader.WriteEntryTo(writableStream);
                }

                ProgressChanged?.Invoke(this, entryNumber / count * 100);

                entryNumber++;
            }
        }).ConfigureAwait(false);
    }
}
