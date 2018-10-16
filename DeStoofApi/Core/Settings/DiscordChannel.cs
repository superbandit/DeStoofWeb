using Core.Serializers;
using Newtonsoft.Json;

namespace Core.Settings
{
    public class DiscordChannel
    {
        [JsonConverter(typeof(SnowflakeConverter))]
        public Snowflake Id { get; }
        public string Name { get; }

        public DiscordChannel(ulong id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
