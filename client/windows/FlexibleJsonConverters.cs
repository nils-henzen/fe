using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FeChat
{
    public class FlexibleStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString() ?? string.Empty;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt64().ToString();
            }
            return string.Empty;
        }
        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }

    public class FlexibleBoolConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.Number:
                    return reader.GetInt32() != 0;
                case JsonTokenType.String:
                    var str = reader.GetString();
                    if (bool.TryParse(str, out var b)) return b;
                    if (int.TryParse(str, out var i)) return i != 0;
                    return false;
                case JsonTokenType.Null:
                    return false;
                default:
                    return false;
            }
        }
        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}

