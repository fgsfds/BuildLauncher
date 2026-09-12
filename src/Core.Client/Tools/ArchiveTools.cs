using Core.Client.Helpers;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Core.Client.Tools;

/// <summary>
///     Provides helpers for unpacking archives.
/// </summary>
public sealed class ArchiveTools
{
    private readonly ILogger<ArchiveTools> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ArchiveTools" /> class.
    /// </summary>
    /// <param name="logger">
    ///     The logger.
    /// </param>
    public ArchiveTools(ILogger<ArchiveTools> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Occurs when unpacking progress changes.
    /// </summary>
    public event EventHandler<float>? ProgressChanged;

    /// <summary>
    ///     Unpacks the specified archive to the target directory.
    /// </summary>
    /// <param name="pathToArchive">
    ///     The path to the archive.
    /// </param>
    /// <param name="unpackTo">
    ///     The directory to unpack into.
    /// </param>
    /// <param name="cancellationToken">
    ///     The cancellation token.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    /// </returns>
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
