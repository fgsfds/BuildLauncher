using Core.Client.Helpers;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Core.Client.Tools;

public sealed class ArchiveTools
{
    private readonly ILogger<ArchiveTools> _logger;

    public ArchiveTools(ILogger<ArchiveTools> logger)
    {
        _logger = logger;
    }

    public event EventHandler<float>? ProgressChanged;

    public async Task UnpackArchiveAsync(
        string pathToArchive,
        string unpackTo,
        CancellationToken cancellationToken = default
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

        Ensure.DirectoryExists(unpackTo);

        _logger.LogInformation(
            "Unpacking archive {PathToArchive} to {UnpackTo}.",
            pathToArchive,
            unpackTo
            );

        using var archive = ArchiveFactory.Open(
            pathToArchive,
            new ReaderOptions
            {
                LeaveStreamOpen = false
            }
            );

        await archive.WriteToDirectoryAsync(
                      unpackTo,
                      progress: progress,
                      cancellationToken: cancellationToken
                      )
                     .ConfigureAwait(false);

        _logger.LogInformation(
            "Unpacked archive {PathToArchive} to {UnpackTo}.",
            pathToArchive,
            unpackTo
            );
    }
}
