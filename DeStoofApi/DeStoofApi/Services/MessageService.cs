using System.Threading.Tasks;
using DeStoofApi.Chatsources.Discord;
using DeStoofApi.Chatsources.Twitch;
using Models.Domain.Messages;

namespace DeStoofApi.Services
{
    public class MessageService
    {
        private readonly DiscordManager _discordManager;
        private readonly TwitchManager _twitchManager;
        private readonly CustomCommandService _customCommandService;

        public MessageService(DiscordManager discordManager, TwitchManager twitchManager, CustomCommandService customCommandService)
        {
            _discordManager = discordManager;
            _twitchManager = twitchManager;
            _customCommandService = customCommandService;
        }

        public async Task OnChatMessageReceived(CustomMessageContext context)
        {
            await _customCommandService.CheckForCustomCommands(context);

            if (context.Message is DiscordChatMessage a && a.ChannelId != context.GuildSettings.TwitchSettings?.DiscordChatChannel?.Id) return;

            await SendToPlatforms(context);
        }

        private async Task SendToPlatforms(CustomMessageContext context)
        {
            var message = context.Message;
            switch (message)
            {
                case TwitchChatMessage _ when context.GuildSettings.TwitchSettings.DiscordChatChannel != null:
                    if (message.Message.Contains("@everyone")) message.Message = message.Message.Replace("@everyone", "/@everyone");
                    if (message.Message.Contains("@here")) message.Message = message.Message.Replace("@here", "/@here");
                    _discordManager.SendMessage(context.GuildSettings.TwitchSettings.DiscordChatChannel.Id, message);

                    break;
                case DiscordChatMessage _ when context.GuildSettings.TwitchSettings.ChannelName != null:
                    await _twitchManager.SendChatMessage(message, context.GuildSettings.TwitchSettings.ChannelName);
                    break;
            }
        }
    }
}
