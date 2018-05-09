namespace DeStoofApi.Models.Guilds
{
    public class GuildTwitchSettings
    {
        public string DiscordChannelname { get; set; }
        public ulong? DiscordChannel { get; set; }
        public string TwitchChannelName { get; set; }
        public int UserId { get; set; }
        public Enums.ChatPlatforms SendTo { get; set; } = Enums.ChatPlatforms.Discord;
    }
}
