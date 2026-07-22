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

    public Task UnpackArchiveAsync(
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

        return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var archive = ArchiveFactory.OpenArchive(
                    pathToArchive,
                    ReaderOptions.ForFilePath
                                 .WithProgress(progress)
                    );

                cancellationToken.ThrowIfCancellationRequested();

                archive.WriteToDirectory(unpackTo);

                cancellationToken.ThrowIfCancellationRequested();
            },
            cancellationToken
            );
    }
}
