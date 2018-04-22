namespace DeStoofApi.Models.ChatMessages
{
    public class DiscordChatMessage : ChatMessage
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
    }
}
