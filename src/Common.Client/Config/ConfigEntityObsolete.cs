using Common.Enums;
using System.Text.Json.Serialization;

namespace Common.Client.Config;

[Obsolete]
public sealed class ConfigEntityObsolete
{
    public ThemeEnum Theme { get; set; }
    public string? GamePathBlood { get; set; }
    public string? GamePathDuke3D { get; set; }
    public string? GamePathDuke64 { get; set; }
    public string? GamePathDukeWT { get; set; }
    public string? GamePathWang { get; set; }
    public string? GamePathRedneck { get; set; }
    public string? GamePathAgain { get; set; }
    public string? GamePathSlave { get; set; }
    public string? GamePathFury { get; set; }
    public bool SkipIntro { get; set; }
    public bool SkipStartup { get; set; }
    public bool UseLocalApi { get; set; }
    public HashSet<string> DisabledAutoloadMods { get; set; } = [];
    public Dictionary<string, TimeSpan> Playtimes { get; set; }
    public Dictionary<string, bool> Upvotes { get; set; }
    public string ApiPassword { get; set; }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters = [typeof(JsonStringEnumConverter<ThemeEnum>)]
    )]
[JsonSerializable(typeof(ConfigEntityObsolete))]
internal sealed partial class ConfigEntityContext : JsonSerializerContext;
