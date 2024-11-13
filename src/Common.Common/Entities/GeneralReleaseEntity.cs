using Common.Enums;
using System.Text.Json.Serialization;

namespace Common.Entities;

public sealed class GeneralReleaseEntity
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
}


[JsonSourceGenerationOptions(
    Converters = [typeof(JsonStringEnumConverter<GameEnum>)]
    )]
[JsonSerializable(typeof(List<GeneralReleaseEntity>))]
public sealed partial class GeneralReleaseEntityContext : JsonSerializerContext;
