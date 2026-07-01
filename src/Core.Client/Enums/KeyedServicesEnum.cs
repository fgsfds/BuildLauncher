namespace Core.Client.Enums;

/// <summary>
///     Identifies keyed dependency injection services used throughout the application.
/// </summary>
public enum KeyedServicesEnum
{
    /// <summary>
    ///     Service key for bitmap caching and retrieval channels.
    /// </summary>
    Bitmaps,

    /// <summary>
    ///     Service key for local file event notification channels.
    /// </summary>
    LocalFilesChannel
}
