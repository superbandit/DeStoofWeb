using System;

namespace Models.Domain.Messages
{
    public abstract class ChatMessage
    {
        public ulong GuildId { get; set; }
        public string User { get; set; }
        public string Message { get; set; }

        public DateTime Date { get; set; }
    }
}
