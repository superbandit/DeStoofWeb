using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeStoofApi.Models.Guilds
{
    public class GuildSettings
    {
        [BsonIgnoreIfDefault]
        public ObjectId Id { get; set; }
        public ulong GuildId { get; set; }
        public string CommandPrefix { get; set; } = "!";
        public GuildTwitchSettings TwitchSettings { get; set; } = new GuildTwitchSettings();
    }
}
