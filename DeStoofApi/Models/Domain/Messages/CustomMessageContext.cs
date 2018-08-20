using Models.Domain.Guilds;

namespace Models.Domain.Messages
{
    public class CustomMessageContext
    {
        public GuildSettings GuildSettings { get; }
        public ChatMessage Message { get; }

        public CustomMessageContext(GuildSettings settings, ChatMessage message)
        {
            GuildSettings = settings;
            Message = message;
        }
    }
}
