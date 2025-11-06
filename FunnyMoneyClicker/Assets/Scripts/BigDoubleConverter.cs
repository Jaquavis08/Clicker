using Newtonsoft.Json;
using BreakInfinity;
using System;

public class BigDoubleConverter : JsonConverter<BigDouble>
{
    public override void WriteJson(JsonWriter writer, BigDouble value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString()); // Always save as string
    }

    public override BigDouble ReadJson(JsonReader reader, Type objectType, BigDouble existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.Value == null) return BigDouble.Zero;

        try
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                    return BigDouble.Parse((string)reader.Value);
                case JsonToken.Float:
                case JsonToken.Integer:
                    return new BigDouble(Convert.ToDouble(reader.Value));
                default:
                    return BigDouble.Zero;
            }
        }
        catch
        {
            return BigDouble.Zero;
        }
    }
}
