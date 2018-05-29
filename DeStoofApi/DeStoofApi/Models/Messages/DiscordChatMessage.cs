namespace DeStoofApi.Models.Messages
{
    public class DiscordChatMessage : ChatMessage
    {
        public ulong UserId { get; set; }
        public ulong ChannelId { get; set; }
    }
}
