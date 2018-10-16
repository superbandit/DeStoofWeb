using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core.Settings
{
    public class GuildSettings
    {
        [JsonProperty]
        private List<CustomCommand> _customCommands;

        public string Id { get; private set; }
        public string GuildId { get; }
        public string CommandPrefix { get; set; }

        public bool StreamMessages { get; set; }

        public TwitchSettings TwitchSettings { get; private set; } 

        public IReadOnlyList<CustomCommand> CustomCommands => _customCommands.AsReadOnly();

        [JsonConstructor]
        private GuildSettings(string guildId, string commandPrefix, TwitchSettings twitchSettings, List<CustomCommand> customCommands, bool streamMessages)
        {
            GuildId = guildId;
            CommandPrefix = commandPrefix;
            TwitchSettings = twitchSettings;
            _customCommands = customCommands;
            StreamMessages = streamMessages;
        }

        public GuildSettings(string guildId, string id = null)
        {
            Id = id;
            GuildId = guildId;
            CommandPrefix = "!";
            _customCommands = new List<CustomCommand>();
        }

        public void SetTwitchChannel(string channelName, string userId)
        {
            TwitchSettings = new TwitchSettings(userId, channelName);
        }

        public void AddCustomCommand(CustomCommand command) => _customCommands.Add(command);
        public bool DeleteCustomCommand(string prefix) => _customCommands.RemoveAll(c => c.Prefix == prefix) > 0;
    }
}
