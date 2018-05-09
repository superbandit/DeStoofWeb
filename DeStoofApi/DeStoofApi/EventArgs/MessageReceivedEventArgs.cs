using DeStoofApi.Models.ChatMessages;

namespace DeStoofApi.EventArgs
{
    public class MessageReceivedEventArgs : System.EventArgs
    {
        public ChatMessage ChatMessage { get; set; }

        public MessageReceivedEventArgs(ChatMessage message)
        {
            ChatMessage = message;
        }
    }
}