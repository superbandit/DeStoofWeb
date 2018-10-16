using System;

namespace Core.Messages
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public IUserMessage Message { get; }
        public IMessagePlatform Platform { get; }

        public MessageReceivedEventArgs(IUserMessage message, IMessagePlatform platform)
        {
            Message = message;
            Platform = platform;
        }
    }
}
