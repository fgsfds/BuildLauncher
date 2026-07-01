using System.Text.Json.Serialization;

namespace Core.All.Serializable;

/// <summary>
///     Represents application data configuration.
/// </summary>
public sealed class DataJson
{
    /// <summary>
    ///     Name of the upload folder.
    /// </summary>
    public const string UploadFolder = "UploadFolder";
}


/// <summary>
///     Source generation context for <see cref="DataJson" /> serialization.
/// </summary>
[JsonSourceGenerationOptions(AllowTrailingCommas = true)]
[JsonSerializable(typeof(Dictionary<string, string>))]
public sealed partial class DataJsonModelContext : JsonSerializerContext;
