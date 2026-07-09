namespace Core.Client.Helpers;

/// <summary>
///     Contains metadata about a remote file including size, last modified date, and URL.
/// </summary>
public readonly struct RemoteFileMetadata
{
    /// <summary>
    ///     File size.
    /// </summary>
    public required readonly long Size { get; init; }

    /// <summary>
    ///     File modified date.
    /// </summary>
    public required readonly DateTime? LastModified { get; init; }

    /// <summary>
    ///     File URL.
    /// </summary>
    public required readonly Uri Url { get; init; }
}
