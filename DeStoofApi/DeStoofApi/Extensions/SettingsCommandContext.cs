using Discord.Commands;
using Discord.WebSocket;
using Models.Domain.Guilds;

namespace DeStoofApi.Extensions
{
    public class SettingsCommandContext : SocketCommandContext
    {
        public SettingsCommandContext(DiscordSocketClient client, SocketUserMessage msg, GuildSettings guildSettings) : base(client, msg)
        {
            GuildSettings = guildSettings;
        }

        public GuildSettings GuildSettings { get; }
    }
}
