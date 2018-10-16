using System;
using Newtonsoft.Json;

namespace Core.Serializers
{
    public class SnowflakeConverter : JsonConverter<Snowflake>
    {
        public override void WriteJson(JsonWriter writer, Snowflake value, JsonSerializer serializer)
        {
            writer.WriteValue(value.RawValue.ToString());
        }

        public override Snowflake ReadJson(JsonReader reader, Type objectType, Snowflake existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            return new Snowflake(ulong.Parse((string)reader.Value));
        }
    }
}
