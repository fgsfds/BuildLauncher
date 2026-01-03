using Common.All.Enums;

namespace Common.All;

public sealed class GeneralRelease
{
    public required OSEnum SupportedOS { get; init; }

    /// <summary>
    /// Release version
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Release description
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Download URL
    /// </summary>
    public required Uri? DownloadUrl { get; init; }

    /// <summary>
    /// File hash
    /// </summary>
    public required string? Hash { get; init; }
}
