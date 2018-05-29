using System.Collections.Generic;
using DeStoofApi.Models.Messages.CustomCommands;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeStoofApi.Models.Guilds
{
    public class GuildSettings
    {
        [BsonIgnoreIfDefault]
        public ObjectId Id { get; set; }

        public ulong GuildId { get; set; }
        public bool Active { get; set; } = true;
        public string CommandPrefix { get; set; } = "!";
        public TwitchSettings TwitchSettings { get; set; } = new TwitchSettings();
        public DiscordSettings DiscordSettings { get; set; } = new DiscordSettings();
        public List<CustomCommand> CustomCommands { get; set; } = new List<CustomCommand>();
    }
}
