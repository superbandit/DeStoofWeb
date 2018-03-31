using System;
using DeStoofApi.Models;

namespace DeStoofApi.EventArguments
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public ChatMessage ChatMessage { get; set; }

        public MessageReceivedEventArgs(ChatMessage message)
        {
            ChatMessage = message;
        }

    }
}