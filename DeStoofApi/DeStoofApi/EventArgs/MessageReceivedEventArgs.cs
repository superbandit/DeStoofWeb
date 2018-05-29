using DeStoofApi.Models.Messages;
using DeStoofApi.Models.Messages.CustomCommands;

namespace DeStoofApi.EventArgs
{
    public class MessageReceivedEventArgs : System.EventArgs
    {
        public ChatMessage ChatMessage { get; }
        public CustomCommandContext CommandContext { get; }

        public MessageReceivedEventArgs(ChatMessage message, CustomCommandContext context)
        {
            ChatMessage = message;
            CommandContext = context;
        }
    }
}