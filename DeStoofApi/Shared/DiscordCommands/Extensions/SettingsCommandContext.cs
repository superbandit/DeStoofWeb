using Core.Settings;
using Discord.Commands;
using Discord.WebSocket;

namespace DeStoofBot.DiscordCommands.Extensions
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
