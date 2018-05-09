namespace DeStoofApi.Models.Twitch
{
    public class TwitchChannel
    {
        public string Channel { get; set; }
        public bool Live { get; set; }
        public ulong GuildId { get; set; }
    }
}
