using System.Text.Json;
using System.Text.Json.Serialization;

namespace Addon.ArtExporter;

/// <summary>Reflection-free serialization of conversion provenance.</summary>
[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ExportReport))]
internal sealed partial class ExportJsonContext : JsonSerializerContext;
