using System;
using Newtonsoft.Json;

namespace Core.Settings
{
    public class TwitchSettings
    {
        public string UserId { get; }
        public string ChannelName { get; }

        public DiscordChannel DiscordChatChannel { get; set; }
        public DiscordChannel DiscordWebhookChannel { get; private set; }
        public string WebhookMessage { get; private set; }

        [JsonConstructor]
        private TwitchSettings(DiscordChannel discordChatChannel, string userId, string channelName, DiscordChannel discordWebhookChannel, string webhookMessage)
        {
            DiscordChatChannel = discordChatChannel;
            UserId = userId;
            ChannelName = channelName;
            DiscordWebhookChannel = discordWebhookChannel;
            WebhookMessage = webhookMessage;
        }

        public TwitchSettings(string userId, string channelname)
        {
            UserId = userId;
            ChannelName = channelname;
        }

        public void SetWebhook(DiscordChannel channel, string message)
        {
            DiscordWebhookChannel = channel ?? throw new ArgumentNullException(nameof(channel));
            WebhookMessage = message;
        }
    }
}
