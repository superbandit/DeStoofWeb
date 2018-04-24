namespace DeStoofApi.Models.Guilds
{
    public class GuildTwitchSettings
    {
        public string DiscordChannelname { get; set; }
        public ulong? DiscordChannel { get; set; }
        public string TwitchChannel { get; set; }
        public Enums.ChatPlatforms SendTo { get; set; } = Enums.ChatPlatforms.Discord;
    }
}
