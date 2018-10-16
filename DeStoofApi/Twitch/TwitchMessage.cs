using Core.Messages;
using TwitchLib.Client.Models;

namespace Twitch
{
    public class TwitchMessage : IUserMessage
    {
        private readonly ChatMessage _message;
        public string SourceId { get; }

        public string Author => _message.DisplayName;
        public string Content => _message.Message;

        public TwitchMessage(ChatMessage message, string sourceId)
        {
            _message = message;
            SourceId = sourceId;
        }
    }
}