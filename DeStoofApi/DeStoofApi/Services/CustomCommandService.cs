using System.Threading.Tasks;
using DeStoofApi.Chatsources.Discord;
using DeStoofApi.Chatsources.Twitch;
using DeStoofApi.Extensions;
using DeStoofApi.Models.Messages;
using DeStoofApi.Models.Messages.CustomCommands;

namespace DeStoofApi.Services
{
    public class CustomCommandService
    {
        private readonly DiscordManager _discordManager;
        private readonly TwitchManager _twitchManager;

        public CustomCommandService(DiscordManager discordManager, TwitchManager twitchManager)
        {
            _discordManager = discordManager;
            _twitchManager = twitchManager;
        }

        public async Task CheckForCustomCommands(ChatMessage message, CustomCommandContext context)
        {
            var compiler = new CustomCommandCompiler(context);

            foreach (var c in context.GuildSettings.CustomCommands)
            {
                if (!message.Message.Contains(c.Prefix)) continue;

                var result = compiler.CompileCustomCommand(c);
                if(message is DiscordChatMessage d) await _discordManager.SendMessage(d.ChannelId, result);
                if (message is TwitchChatMessage t) await _twitchManager.SendMessage(result, t.Channel);
            }
        }
    }
}
