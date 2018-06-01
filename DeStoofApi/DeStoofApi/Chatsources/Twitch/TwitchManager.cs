using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeStoofApi.EventArgs;
using DeStoofApi.Models.Guilds;
using DeStoofApi.Models.Messages;
using DeStoofApi.Models.Messages.CustomCommands;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using TwitchLib.Api;
using TwitchLib.Api.Enums;
using TwitchLib.Api.Models.Helix.Users.GetUsers;
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

        private readonly TwitchAPI _twitchApi = new TwitchAPI();
        private readonly TwitchClient _client = new TwitchClient();

        private readonly IConfiguration _config;
        private readonly IMongoCollection<GuildSettings> _guildSettings;

        public TwitchManager(IConfiguration config, IMongoDatabase database)
        {
            _config = config;

            _guildSettings = database.GetCollection<GuildSettings>(config["Secure:GuildSettings"]);
        }

        public void Start()
        {
            _client.Initialize(new ConnectionCredentials("StreamerCompanion", _config["Secure:TwitchToken"]));
            _twitchApi.Settings.ClientId = "yjg7ikvnh54s37xgxao69ufub0nv4b";
            _twitchApi.Settings.AccessToken = _config["Secure:TwitchApiToken"];

            _client.OnMessageReceived += OnMessageReceived;
            _client.AutoReListenOnException = true;
            _client.OnDisconnected += OnDisconnected;

            _client.Connect();
        }

        private void OnDisconnected(object sender, OnDisconnectedArgs e)
        {
            _client.Reconnect();
        }

        public bool JoinTwitchChannel(string channel)
        {

            if (_client.JoinedChannels.Any(c => c.Channel == channel)) return false;

            _client.JoinChannel(channel);
            return true;
        }

        public bool LeaveTwitchChannel(string channel)
        {
            if (_client.JoinedChannels.All(c => c.Channel != channel)) return false;

            _client.LeaveChannel(channel);
            return true;
        }

        public async Task<GetUsersResponse> GetChannelusers(List<string> channelNames)
        {
            return await _twitchApi.Users.helix.GetUsersAsync(logins: channelNames);
        }

        public Task<bool> SubToChannelLiveWebhook(int userId)
        {
            return _twitchApi.Webhooks.helix.StreamUpDownAsync(
                "http://destoofapi.azurewebsites.net/api/chat/channelLive", WebhookCallMode.Subscribe, userId.ToString(), new TimeSpan(0,0,0, 864000));
        }

        private async void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            ChatMessage message = e.ChatMessage;

            if (message.IsMe) return;

            var settings = await (await _guildSettings.FindAsync(g => g.TwitchSettings.TwitchChannelName.ToLower() == message.Channel.ToLower())).FirstOrDefaultAsync();
            
            var chatMessage = new TwitchChatMessage
            {
                GuildId = settings.GuildId,
                Channel = message.Channel,
                Date = DateTime.Now,
                Message = message.Message,
                User = message.DisplayName,
                SendTo = settings.TwitchSettings.SendTo
            };

            var context = new CustomCommandContext(settings, chatMessage);

            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(chatMessage, context));
        }

        public Task SendChatMessage(Models.Messages.ChatMessage message, string channel)
        {
            if (_client.JoinedChannels.Any(c => c.Channel == channel))
                _client.SendMessage(channel, $"{message.User}: {message.Message}");

            return Task.CompletedTask;
        }

        public Task SendMessage(string message, string channel)
        {
            if (_client.JoinedChannels.Any(c => c.Channel == channel))
                _client.SendMessage(channel, message);

            return Task.CompletedTask;
        }
    }
}