using Core;
using Discord.WebSocket;

namespace Discord
{
    public class DiscordMessage : Core.Messages.IUserMessage
    {
        private readonly SocketMessage _message;
        private readonly Snowflake _snowflake;

        public string Author => ((IGuildUser) _message.Author).Nickname ?? _message.Author.Username;
        public string Content => _message.Content;
        public string SourceId => _snowflake.RawValue.ToString();

        public DiscordMessage(SocketMessage message)
        {
            _message = message;
            _snowflake = message.Channel.Id;
        }
    }
}
