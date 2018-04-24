namespace DeStoofApi.Models.Guilds
{
    public class DiscordSettings
    {
        public Enums.ChatPlatforms SendTo { get; set; } = Enums.ChatPlatforms.Twitch;
    }
}