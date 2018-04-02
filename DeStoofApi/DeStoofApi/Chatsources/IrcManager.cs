using DeStoofApi.EventArguments;
using DeStoofApi.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DeStoofApi.Chatsources
{
    public class IrcManager
    {
        public delegate void MessageReceivedHandler(object sender, MessageReceivedEventArgs args);
        public event MessageReceivedHandler MessageReceived;

        readonly IConfiguration Config;

        private List<TwitchSource> ChatConnections;

        public IrcManager(IConfiguration config)
        {
            Config = config;

            ChatConnections = new List<TwitchSource>();
        }

        public bool StartConnection(string channel)
        {
            if (!ChatConnections.Exists(x => x.channel == channel))
            {
                var twitchSource = new TwitchSource("irc.twitch.tv", 6667, "DeStoofBot", $"{Config["Secure:TwitchToken"]}", channel);
                ChatConnections.Add(twitchSource);
                twitchSource.backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(ReceiveMessage);
                twitchSource.Connect();

                return true;
            }
            return false;
        }

        public void StopConnection(string chanel)
        {
            ChatConnections.Find(x => x.channel == chanel).Disconnect();
        }

        public void SendMessage(string chanel, string message)
        {
            ChatConnections.Find(x => x.channel == chanel).SendPublicChatMessage(message);
        }

        public void ReceiveMessage(object sender, ProgressChangedEventArgs e)
        {
            string message = (string)e.UserState;
            if (message.Contains("PRIVMSG"))
            {
                ChatMessage chatMessage = CreateChatMessage(message);

                MessageReceived(this, new MessageReceivedEventArgs(chatMessage));
            }
        }

        private ChatMessage CreateChatMessage(string message)
        {
            ChatMessage chatMessage = new ChatMessage();
            string[] components = message.Split(' ');
            string[] information = components[0].Split('!');
            chatMessage.Channel = components[2].Substring(1);
            chatMessage.User = information[0].Substring(1);
            chatMessage.Message = string.Join(" ", components, 3, components.Length - 3).Substring(1);

            chatMessage.Date = DateTime.Now.ToString();
            chatMessage.Platform = Enums.Platforms.twitch;

            return chatMessage;
        }
    }
}
