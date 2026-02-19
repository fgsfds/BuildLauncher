using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

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
        var progress = new Progress<ProgressReport>(report =>
        {
            if (report.PercentComplete is not null)
            {
                ProgressChanged?.Invoke(this, (float)report.PercentComplete.Value);
            }
        });

        if (!Directory.Exists(unpackTo))
        {
            Directory.CreateDirectory(unpackTo);
        }

        using var archive = ArchiveFactory.OpenArchive(
            pathToArchive,
            ReaderOptions.ForOwnedFile
            .WithProgress(progress)
            );

        archive.WriteToDirectory(unpackTo);
    }
}
