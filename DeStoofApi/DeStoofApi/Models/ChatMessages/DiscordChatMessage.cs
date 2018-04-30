namespace DeStoofApi.Models.ChatMessages
{
    public class DiscordChatMessage : ChatMessage
    {
        public ulong UserId { get; set; }
        public ulong ChannelId { get; set; }
    }
}
