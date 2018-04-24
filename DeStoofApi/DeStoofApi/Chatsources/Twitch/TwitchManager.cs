using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeStoofApi.EventArgs;
using DeStoofApi.Models.ChatMessages;
using DeStoofApi.Models.Guilds;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using ChatMessage = TwitchLib.Client.Models.ChatMessage;

namespace DeStoofApi.Chatsources.Twitch
{
    public class TwitchManager
    {
        public delegate Task MessageReceivedHandler(object sender, MessageReceivedEventArgs args);
        public event MessageReceivedHandler MessageReceived;

        private TwitchClient _client;
        private readonly ConnectionCredentials _credentials;
        private readonly IMongoCollection<GuildSettings> _guildSettings;
        private readonly List<string> _connectedChannels = new List<string>();

        public TwitchManager(IConfiguration config, IMongoDatabase database)
        {
            _guildSettings = database.GetCollection<GuildSettings>(config["Secure:GuildSettings"]);
            _credentials = new ConnectionCredentials("destoofbot", config["Secure:TwitchToken"]);
        }

        public void Start()
        {
            _client = new TwitchClient();
            _client.Initialize(_credentials);

            _client.OnMessageReceived += OnMessageReceived;

            _client.Connect();
        }

        public bool JoinTwitchChannel(string channel)
        {
            if (_connectedChannels.Contains(channel)) return false;

            _connectedChannels.Add(channel);
            _client.JoinChannel(channel);
            return true;

        }

        public bool LeaveTwitchChannel(string channel)
        {
            if (!_connectedChannels.Contains(channel)) return false;

            _connectedChannels.Remove(channel);
            _client.LeaveChannel(channel);
            return true;

        }

        private async void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            ChatMessage message = e.ChatMessage;

            if (message.IsMe) return;

            //Handle twitch commands
            var settings = await (await _guildSettings.FindAsync(g => g.TwitchSettings.TwitchChannel == message.Channel)).FirstOrDefaultAsync();
            

            TwitchChatMessage chatMessage = new TwitchChatMessage
            {
                Channel = message.Channel,
                Date = DateTime.Now,
                Message = message.Message,
                User = message.DisplayName,
                SendTo = settings.TwitchSettings.SendTo
            };
            chatMessage.GuildIds.Add(settings.GuildId);

            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(chatMessage));
        }

        public Task SendMessage(Models.ChatMessages.ChatMessage message, string channel)
        {
            if (_connectedChannels.Contains(channel))
            {
                _client.SendMessage(channel, $"{message.User}: {message.Message}");               
            }
            return Task.CompletedTask;
        }
    }
}