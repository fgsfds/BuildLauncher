using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Core.Client.Tools;

/// <summary>
///     Provides archive extraction functionality with progress reporting.
/// </summary>
public sealed class ArchiveTools
{
    private readonly ILogger<ArchiveTools> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ArchiveTools" /> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public ArchiveTools(ILogger<ArchiveTools> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Operation progress
    /// </summary>
    public event EventHandler<float>? ProgressChanged;

    /// <summary>
    ///     Unpack archive
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
            }
            );

        if (!Directory.Exists(unpackTo))
        {
            Directory.CreateDirectory(unpackTo);
        }

        using var archive = ArchiveFactory.OpenArchive(
            pathToArchive,
            ReaderOptions.ForFilePath
                         .WithProgress(progress)
            );

        archive.WriteToDirectory(unpackTo);
    }
}
