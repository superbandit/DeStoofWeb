using System;
using System.Collections.Generic;
using DeStoofApi.EventArgs;
using DeStoofApi.Models.ChatMessages;
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

        private List<string> connectedChannels = new List<string>();

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
            if (connectedChannels.Contains(channel)) return false;

            connectedChannels.Add(channel);
            _client.JoinChannel(channel);
            return true;

        }

        public bool LeaveTwitchChannel(string channel)
        {
            if (!connectedChannels.Contains(channel)) return false;

            connectedChannels.Remove(channel);
            _client.LeaveChannel(channel);
            return true;

        }

        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            ChatMessage message = e.ChatMessage;

            if (message.IsMe) return;

            //Handle twitch commands

            TwitchChatMessage chatMessage = new TwitchChatMessage
            {
                Channel = message.Channel,
                Date = DateTime.Now,
                Message = message.Message,
                User = message.DisplayName
            };

            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(chatMessage));
        }
    }
}