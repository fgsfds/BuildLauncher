using Common.Enums;
using CommunityToolkit.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Common.Helpers;

public class GameEnumJsonConverter : JsonConverter<GameEnum>
{
    public override GameEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        Guard.IsNotNullOrWhiteSpace(value);

        if (Enum.TryParse<GameEnum>(value, true, out var gameEnum))
        {
            return gameEnum;
        }

        if (value.Equals("ShadowWarrior", StringComparison.OrdinalIgnoreCase))
        {
            return GameEnum.Wang;
        }
        else if (value.Equals("Exhumed", StringComparison.OrdinalIgnoreCase))
        {
            return GameEnum.Slave;
        }
        else
        {
            return ThrowHelper.ThrowArgumentOutOfRangeException<GameEnum>(value);
        }
    }

    public override void Write(Utf8JsonWriter writer, GameEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }

    public override GameEnum ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => Read(ref reader, typeToConvert, options);

    public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] GameEnum value, JsonSerializerOptions options)
        => Write(writer, value, options);
}