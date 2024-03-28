using Common.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mods.Serializable
{
    public sealed class StringOrDescriptionConverter : JsonConverter<Description>
    {
        public override Description? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is JsonTokenType.String)
            {
                var text = reader.GetString();

                return new() { IsText = true, Text = text! };
            }

            if (reader.TokenType is JsonTokenType.StartObject)
            {
                try
                {
                    var a = JsonSerializer.Deserialize(ref reader, DescriptionContext.Default.DescriptionPath);

                    return new() { IsText = false, Text = a!.PathToTxt };
                }
                catch { }
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, Description value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }


    public sealed class SingleOrArrayConverter<TItem> : JsonConverter<List<TItem>>
    {
        public SingleOrArrayConverter() : this(true) { }
        public SingleOrArrayConverter(bool canWrite) => CanWrite = canWrite;

        public bool CanWrite { get; }

        public override List<TItem>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.StartArray:
                    var list = new List<TItem>();

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndArray)
                        {
                            break;
                        }

                        var result1 = GetItem(ref reader);

                        if (result1 is not null)
                        {
                            list.Add(result1);
                        }
                    }

                    return list;

                default:
                    var result = GetItem(ref reader);

                    if (result is null)
                    {
                        return null;
                    }

                    return [result];
            }
        }

        public override void Write(Utf8JsonWriter writer, List<TItem> value, JsonSerializerOptions options)
        {
            if (CanWrite && value.Count == 1)
            {
                JsonSerializer.Serialize(writer, value.First(), options);
            }
            else
            {
                throw new NotImplementedException();

                //writer.WriteStartArray();
                //foreach (var item in value)
                //{
                //    JsonSerializer.Serialize(writer, item, options);
                //}

                //writer.WriteEndArray();
            }
        }

        private TItem? GetItem(ref Utf8JsonReader reader)
        {
            if (typeof(TItem) == typeof(string))
            {
                return (TItem)(object)reader.GetString()!;
            }
            else if (typeof(TItem) == typeof(int))
            {
                if (reader.TokenType is JsonTokenType.Number)
                {
                    return (TItem)(object)reader.GetInt32();
                }
                else if (reader.TokenType is JsonTokenType.String)
                {
                    var str = reader.GetString();
                    var intValue = Convert.ToInt32(str, 16);

                    return (TItem)(object)intValue;
                }
            }
            else if (typeof(TItem) == typeof(Typed))
            {
                return (TItem)(object)JsonSerializer.Deserialize(ref reader, TypedContext.Default.Typed)!;
            }
            else if (typeof(TItem) == typeof(Addon))
            {
                return (TItem)(object)JsonSerializer.Deserialize(ref reader, AddonContext.Default.Addon)!;
            }
            else if (typeof(TItem) == typeof(GameEnum))
            {
                var str = reader.GetString();

                return (TItem)Enum.Parse(typeof(GameEnum), str!, true);
            }
            else
            {
                throw new NotImplementedException();
            }

            return default;
        }

    }
}
