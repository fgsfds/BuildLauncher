using System.Text.Json.Serialization;

namespace Common.Entities;

public sealed class GeneralReleaseEntity
{
    /// <summary>
    /// Release version
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Release description
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Windows release download URL
    /// </summary>
    public required Uri? WindowsDownloadUrl { get; init; }

    /// <summary>
    /// Linux release download URL
    /// </summary>
    public required Uri? LinuxDownloadUrl { get; init; }
}

[JsonSerializable(typeof(List<GeneralReleaseEntity>))]
public sealed partial class GeneralReleaseEntityContext : JsonSerializerContext;
