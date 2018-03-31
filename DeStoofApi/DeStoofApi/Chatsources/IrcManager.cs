using DeStoofApi.EventArguments;
using DeStoofApi.Models;
using System.ComponentModel;


namespace DeStoofApi.Chatsources
{
    public class IrcManager
    {
        public delegate void MessageReceivedHandler(object sender, MessageReceivedEventArgs args);
        public event MessageReceivedHandler MessageReceived;

        TwitchSource ChatConnection;
        BackgroundWorker BackgroundWorker;

        public IrcManager()
        {
            ChatConnection = new TwitchSource("irc.twitch.tv", 6667, "DeStoofBot", "oauth:md7lxoimmj5x7fc3zoi3ev7i5ii0pg", "destoofpot");

            BackgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            BackgroundWorker.DoWork += new DoWorkEventHandler(ChatConnection.GetMessages);
            BackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(ReceiveMessage);

            BackgroundWorker.RunWorkerAsync();
        }

        public void StopConnection()
        {
            BackgroundWorker.CancelAsync();
            ChatConnection.Disconnect();
        }

        public void SendMessage(string message)
        {
            ChatConnection.SendPublicChatMessage(message);
        }

        public void ReceiveMessage(object sender, ProgressChangedEventArgs e)
        {
            string message = (string)e.UserState;
            if (message.Contains("PRIVMSG"))
            {
                ChatMessage chatMessage = DisectMessage(message);

                MessageReceived(this, new MessageReceivedEventArgs(chatMessage));
            }
        }

        private ChatMessage DisectMessage(string message)
        {
            ChatMessage chatMessage = new ChatMessage();
            string[] components = message.Split(' ');
            string[] information = components[0].Split('!');
            chatMessage.Channel = components[2].Substring(1);
            chatMessage.User = information[0].Substring(1);
            chatMessage.Message = string.Join(" ", components, 3, components.Length - 3).Substring(1);

            return chatMessage;
        }
    }
}
