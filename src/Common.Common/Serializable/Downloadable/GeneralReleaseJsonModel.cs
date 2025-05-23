using System.Text.Json.Serialization;
using Common.Enums;

namespace Common.Common.Serializable.Downloadable;

public sealed class GeneralReleaseJsonModel
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
[JsonSerializable(typeof(List<GeneralReleaseJsonModel>))]
public sealed partial class GeneralReleaseJsonModelContext : JsonSerializerContext;
