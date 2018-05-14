namespace DeStoofApi.Models.Guilds
{
    public class TwitchSettings
    {
        public string DiscordChannelname { get; set; }
        public ulong? DiscordChannel { get; set; } = null;
        public string TwitchChannelName { get; set; }
        public ulong? WebhookDiscordChannel { get; set; } = null;
        public string WebhookDiscordChannelName { get; set; }
        public string WebhookMessage { get; set; }
        public int? UserId { get; set; } = null;
        public Enums.ChatPlatforms SendTo { get; set; } = Enums.ChatPlatforms.Discord;
    }
}
