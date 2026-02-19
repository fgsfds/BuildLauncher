using System.Text.Json;
using System.Text.Json.Serialization;
using Common.All.Enums;
using Common.All.Interfaces;
using Common.All.Serializable.Addon;

namespace Common.All.Serializable;

public sealed class SupportedGameDtoConverter : JsonConverter<SupportedGameJsonModel?>
{
    public override SupportedGameJsonModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.StartObject)
        {
            return JsonSerializer.Deserialize(ref reader, SupportedGameJsonModelContext.Default.SupportedGameJsonModel);
        }
        else if (reader.TokenType is JsonTokenType.String)
        {
            var str = reader.GetString();

            if (str is null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            var gameEnum = Enum.Parse<GameEnum>(str, true);

            SupportedGameJsonModel dto = new()
            {
                Game = gameEnum
            };

            return dto;
        }

        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, SupportedGameJsonModel? value, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }
}


public sealed class IStartMapConverter : JsonConverter<IStartMap?>
{
    public override IStartMap? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.StartObject)
        {
            try
            {
                return JsonSerializer.Deserialize(ref reader, MapFileJsonModelContext.Default.MapFileJsonModel);
            }
            catch { }

            try
            {
                return JsonSerializer.Deserialize(ref reader, MapSlotJsonModelContext.Default.MapSlotJsonModel);
            }
            catch { }
        }

        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, IStartMap? value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value is MapFileJsonModel fileMap)
        {
            writer.WritePropertyName("file");
            writer.WriteStringValue(fileMap.File);
        }
        else if (value is MapSlotJsonModel slotMap)
        {
            writer.WritePropertyName("volume");
            writer.WriteNumberValue(slotMap.Episode);
            writer.WritePropertyName("level");
            writer.WriteNumberValue(slotMap.Level);
        }
        else
        {
            throw new NotSupportedException();
        }

        writer.WriteEndObject();
    }
}

public sealed class ExecutablesConverter : JsonConverter<Dictionary<OSEnum, Dictionary<PortEnum, string>>?>
{
    public override Dictionary<OSEnum, Dictionary<PortEnum, string>>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.StartObject)
        {
            try
            {
                return JsonSerializer.Deserialize(ref reader, AddonManifestContext.Default.DictionaryOSEnumDictionaryPortEnumString);
            }
            catch { }

            try
            {
                var old = JsonSerializer.Deserialize(ref reader, AddonManifestContext.Default.DictionaryOSEnumString);

                if (old is not null)
                {
                    Dictionary<OSEnum, Dictionary<PortEnum, string>> result = [];

                    if (old.TryGetValue(OSEnum.Windows, out var winPort))
                    {
                        result.Add(OSEnum.Windows, []);

                        if (winPort.StartsWith("nblood"))
                        {
                            result[OSEnum.Windows].Add(PortEnum.NBlood, winPort);
                        }
                        else if (winPort.StartsWith("notblood"))
                        {
                            result[OSEnum.Windows].Add(PortEnum.NotBlood, winPort);
                        }
                        else if (winPort.StartsWith("eduke"))
                        {
                            result[OSEnum.Windows].Add(PortEnum.EDuke32, winPort);
                        }
                        else if (winPort.StartsWith("raze"))
                        {
                            result[OSEnum.Windows].Add(PortEnum.Raze, winPort);
                        }
                        else
                        {
                            result[OSEnum.Windows].Add(PortEnum.Stub, winPort);
                        }
                    }

                    if (old.TryGetValue(OSEnum.Linux, out var linPort))
                    {
                        result.Add(OSEnum.Linux, []);

                        if (linPort.StartsWith("nblood"))
                        {
                            result[OSEnum.Linux].Add(PortEnum.NBlood, linPort);
                        }
                        else if (linPort.StartsWith("notblood"))
                        {
                            result[OSEnum.Linux].Add(PortEnum.NotBlood, linPort);
                        }
                        else if (linPort.StartsWith("eduke"))
                        {
                            result[OSEnum.Linux].Add(PortEnum.EDuke32, linPort);
                        }
                    }

                    return result;
                }

                return null;
            }
            catch { }
        }

        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<OSEnum, Dictionary<PortEnum, string>>? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        foreach (var osEntry in value)
        {
            writer.WritePropertyName(osEntry.Key.ToString());
            writer.WriteStartObject();

            foreach (var portEntry in osEntry.Value)
            {
                writer.WriteString(portEntry.Key.ToString(), portEntry.Value);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndObject();
    }
}


//public sealed class SingleOrArrayConverter<TItem> : JsonConverter<List<TItem>>
//{
//    public SingleOrArrayConverter() : this(true) { }
//    public SingleOrArrayConverter(bool canWrite) => CanWrite = canWrite;

//    public bool CanWrite { get; }

//    public override List<TItem>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        switch (reader.TokenType)
//        {
//            case JsonTokenType.Null:
//                return null;

//            case JsonTokenType.StartArray:
//                var list = new List<TItem>();

//                while (reader.Read())
//                {
//                    if (reader.TokenType == JsonTokenType.EndArray)
//                    {
//                        break;
//                    }

//                    var result1 = GetItem(ref reader);

//                    if (result1 is not null)
//                    {
//                        list.Add(result1);
//                    }
//                }

//                return list;

//            default:
//                var result = GetItem(ref reader);

//                if (result is null)
//                {
//                    return null;
//                }

//                return [result];
//        }
//    }

//    public override void Write(Utf8JsonWriter writer, List<TItem> value, JsonSerializerOptions options)
//    {
//        if (CanWrite && value.Count == 1)
//        {
//            JsonSerializer.Serialize(writer, value.First(), options);
//        }
//        else
//        {
//            throw new NotSupportedException();
//        }
//    }

//    private TItem? GetItem(ref Utf8JsonReader reader)
//    {
//        if (typeof(TItem) == typeof(string))
//        {
//            return (TItem)(object)reader.GetString()!;
//        }
//        else if (typeof(TItem) == typeof(int))
//        {
//            if (reader.TokenType is JsonTokenType.Number)
//            {
//                return (TItem)(object)reader.GetInt32();
//            }
//            else if (reader.TokenType is JsonTokenType.String)
//            {
//                var str = reader.GetString();
//                var intValue = Convert.ToInt32(str, 16);

//                return (TItem)(object)intValue;
//            }
//        }
//        else if (typeof(TItem) == typeof(GameEnum))
//        {
//            var str = reader.GetString();

//            return (TItem)Enum.Parse(typeof(GameEnum), str!, true);
//        }
//        else if (typeof(TItem) == typeof(PortEnum))
//        {
//            var str = reader.GetString();

//            return (TItem)Enum.Parse(typeof(PortEnum), str!, true);
//        }
//        else if (typeof(TItem) == typeof(FeatureEnum))
//        {
//            var str = reader.GetString();

//            return (TItem)Enum.Parse(typeof(FeatureEnum), str!, true);
//        }
//        else
//        {
//            throw new NotSupportedException();
//        }

//        return default;
//    }
//}

public sealed class GameEnumJsonConverter : JsonConverter<GameEnum>
{
    public override GameEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

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
            throw new NotSupportedException(value);
        }
    }

    public override void Write(Utf8JsonWriter writer, GameEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }

    public override GameEnum ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => Read(ref reader, typeToConvert, options);

    public override void WriteAsPropertyName(Utf8JsonWriter writer, GameEnum value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.ToString());
    }
}
