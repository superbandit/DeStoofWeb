using Core.Messages;

namespace Discord
{
    public class DiscordMessageSanitizer : IMessageSanitizer
    {
        public string Sanitize(string content) => content.Replace("@", "@ ");
    }
}