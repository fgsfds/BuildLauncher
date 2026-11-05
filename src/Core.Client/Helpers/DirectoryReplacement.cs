using Microsoft.Extensions.Logging;

namespace Core.Client.Helpers;

/// <summary> Prepares a sibling directory and replaces its destination with rollback on failed moves. </summary>
public sealed class DirectoryReplacement : IDisposable
{
    /// <summary> The live destination. </summary>
    private readonly string _destination;

    /// <summary> The diagnostic logger. </summary>
    private readonly ILogger _logger;

    /// <summary> Creates an isolated staging directory beside the destination. </summary>
    /// <param name="destination">The live directory to replace.</param>
    /// <param name="logger">The diagnostic logger.</param>
    public DirectoryReplacement(string destination, ILogger logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        _destination = Path.TrimEndingDirectorySeparator(Path.GetFullPath(destination));
        if (Path.GetDirectoryName(_destination) is null)
        {
            throw new ArgumentException("An installation cannot replace a filesystem root.", nameof(destination));
        }
        _logger = logger;
        StagingPath = _destination + ".staging-" + Guid.NewGuid().ToString("N");
        _ = Directory.CreateDirectory(StagingPath);
    }

    /// <summary> Gets the isolated directory in which callers prepare content. </summary>
    public string StagingPath { get; }

    /// <summary> Copies existing content so updates preserve configuration and user files. </summary>
    /// <param name="cancellationToken">Cancels copying before live content is changed.</param>
    public void CopyExisting(CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(_destination))
        {
            CopyDirectory(_destination, StagingPath, cancellationToken);
        }
    }

    /// <summary> Copies a directory without following links outside the installation. </summary>
    /// <param name="source">The source directory.</param>
    /// <param name="destination">The staging directory.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private static void CopyDirectory(string source, string destination, CancellationToken cancellationToken)
    {
        if ((File.GetAttributes(source) & FileAttributes.ReparsePoint) != 0)
        {
            throw new IOException($"Cannot stage a linked installation directory: {source}");
        }

        _ = Directory.CreateDirectory(destination);
        foreach (var entry in Directory.EnumerateFileSystemEntries(source))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var attributes = File.GetAttributes(entry);
            if ((attributes & FileAttributes.ReparsePoint) != 0)
            {
                throw new IOException($"Cannot stage a linked installation entry: {entry}");
            }

            var target = Path.Combine(destination, Path.GetFileName(entry));
            if ((attributes & FileAttributes.Directory) != 0)
            {
                CopyDirectory(entry, target, cancellationToken);
            }
            else
            {
                File.Copy(entry, target);
            }
        }
    }

    /// <summary> Commits prepared content, retaining the old directory if rollback itself fails. </summary>
    public void Commit()
    {
        var backup = _destination + ".backup-" + Guid.NewGuid().ToString("N");
        var hadDestination = Directory.Exists(_destination);
        if (hadDestination)
        {
            Directory.Move(_destination, backup);
        }

        try
        {
            Directory.Move(StagingPath, _destination);
        }
        catch (Exception failure)
        {
            if (hadDestination)
            {
                try
                {
                    Directory.Move(backup, _destination);
                }
                catch (Exception rollbackFailure)
                {
                    throw new AggregateException($"Replacement failed. Original files remain at {backup}.", failure, rollbackFailure);
                }
            }

            throw;
        }

        TryDelete(backup);
    }

    /// <summary> Removes only this operation's staging directory. </summary>
    public void Dispose() => TryDelete(StagingPath);

    /// <summary> Cleans temporary content without hiding the operation outcome. </summary>
    /// <param name="path">The operation-owned directory.</param>
    private void TryDelete(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not remove temporary directory {Path}.", path);
        }
    }
}
