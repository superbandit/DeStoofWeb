namespace Models.Domain.Messages
{
    public class DiscordChatMessage : ChatMessage
    {
        public ulong UserId { get; set; }
        public ulong ChannelId { get; set; }
    }
}
