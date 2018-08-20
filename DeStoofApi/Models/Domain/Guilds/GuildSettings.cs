using System.Collections.Generic;
using Models.Interfaces;
using Newtonsoft.Json;

namespace Models.Domain.Guilds
{
    public class GuildSettings : IBaseEntity
    {
        public string Id { get; private set; }
        // Raven fucks up ulongs
        public string GuildId { get; }

        public string CommandPrefix { get; set; }
        public TwitchSettings TwitchSettings { get; private set; } 
        public DiscordSettings DiscordSettings { get; }
        public List<CustomCommand> CustomCommands { get; }

        [JsonConstructor]
        private GuildSettings(string guildId, string commandPrefix, TwitchSettings twitchSettings, DiscordSettings discordSettings, List<CustomCommand> customCommands)
        {
            GuildId = guildId;
            CommandPrefix = commandPrefix;
            TwitchSettings = twitchSettings;
            DiscordSettings = discordSettings;
            CustomCommands = customCommands;
        }

        public GuildSettings(ulong guildId, string id = null)
        {
            Id = id;
            GuildId = guildId.ToString();
            CommandPrefix = "!";
            DiscordSettings = new DiscordSettings();
            CustomCommands = new List<CustomCommand>();
        }

        public void SetTwitchChannel(string channelName, string userId)
        {
            TwitchSettings = new TwitchSettings(userId, channelName);
        }
    }
}
