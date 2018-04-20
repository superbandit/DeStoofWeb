using System;
using DeStoofApi.EventArgs;
using DeStoofApi.Models;
using Microsoft.Extensions.Configuration;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using ChatMessage = TwitchLib.Client.Models.ChatMessage;

namespace DeStoofApi.Chatsources.Twitch
{
    public class TwitchManager
    {
        public delegate void MessageReceivedHandler(object sender, MessageReceivedEventArgs args);
        public event MessageReceivedHandler MessageReceived;

        TwitchClient _client;
        private ConnectionCredentials credentials;

        public TwitchManager(IConfiguration config)
        {
            credentials = new ConnectionCredentials("destoofbot", config["Secure:TwitchToken"]);
        }

        public void Start()
        {

            _client = new TwitchClient();
            _client.Initialize(credentials);

            _client.OnMessageReceived += OnMessageReceived;

            _client.Connect();
        }

        public bool JoinTwitchChannel(string channel)
        {
            _client.JoinChannel(channel);

            return true;
        }

        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            ChatMessage message = e.ChatMessage;

            if (message.IsMe) return;

            //Handle twitch commands

            Models.ChatMessage chatMessage = new Models.ChatMessage
            {
                Channel = message.Channel,
                Date = DateTime.Now.ToString(),
                Message = message.Message,
                Platform = Enums.Platforms.Twitch,
                User = message.DisplayName
            };

            if (MessageReceived != null) MessageReceived(this, new MessageReceivedEventArgs(chatMessage));
        }
    }
}