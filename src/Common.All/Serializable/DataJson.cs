using System.Text.Json.Serialization;

namespace Common.All.Serializable;

public sealed class DataJson
{
    public const string UploadFolder = "UploadFolder";
}


[JsonSourceGenerationOptions(AllowTrailingCommas = true)]
[JsonSerializable(typeof(Dictionary<string, string>))]
public sealed partial class DataJsonModelContext : JsonSerializerContext;
