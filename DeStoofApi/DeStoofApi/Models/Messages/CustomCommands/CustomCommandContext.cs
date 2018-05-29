using DeStoofApi.Models.Guilds;

namespace DeStoofApi.Models.Messages.CustomCommands
{
    public class CustomCommandContext
    {
        public GuildSettings GuildSettings { get; }
        public string Caller { get; }

        public CustomCommandContext(GuildSettings settings, ChatMessage message)
        {
            GuildSettings = settings;
            Caller = message.User;
        }
    }
}
