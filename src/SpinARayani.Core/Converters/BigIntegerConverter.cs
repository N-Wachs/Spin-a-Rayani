using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpinARayani.Core.Converters;

public class BigIntegerConverter : JsonConverter<BigInteger>
{
    public override BigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return new BigInteger(reader.GetInt64());
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (BigInteger.TryParse(str, out var value))
            {
                return value;
            }
        }
        return BigInteger.Zero;
    }

    public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
