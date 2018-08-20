using Newtonsoft.Json;

namespace Models.Domain.Guilds
{
    public class DiscordChannel
    {
        [JsonIgnore] public ulong Id { get; }
        [JsonProperty] private string IdString { get; }
        public string Name { get; }

        [JsonConstructor]
        private DiscordChannel(string idString, string name)
        {
            Id = ulong.Parse(idString);
            IdString = idString;
            Name = name;
        }

        public DiscordChannel(ulong id, string name)
        {
            Id = id;
            IdString = id.ToString();
            Name = name;
        }
    }
}
