using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TwitchLib.Api;
using TwitchLib.Api.Enums;
using TwitchLib.Api.Models.Helix.Users.GetUsers;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace DeStoofApi.Chatsources.Twitch
{
    public class TwitchManager
    {
        private readonly TwitchAPI _twitchApi = new TwitchAPI();
        private readonly TwitchClient _client;

        private readonly IConfiguration _config;

        public TwitchManager(IConfiguration config, TwitchClient client)
        {
            _config = config;
            _client = client;
        }

        public void Start()
        {
            _client.Initialize(new ConnectionCredentials("StreamerCompanion", _config["Secure:TwitchToken"]));
            _twitchApi.Settings.ClientId = "yjg7ikvnh54s37xgxao69ufub0nv4b";
            _twitchApi.Settings.AccessToken = _config["Secure:TwitchApiToken"];

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

        public bool TryLeaveTwitchChannel(string channel)
        {
            if (_client.JoinedChannels.All(c => c.Channel != channel)) return false;

            _client.LeaveChannel(channel);
            return true;
        }

        public async Task<GetUsersResponse> GetChannelusers(List<string> channelNames)
        {
            return await _twitchApi.Users.helix.GetUsersAsync(logins: channelNames);
        }

        public Task<bool> SubToChannelLiveWebhook(string userId)
        {
            return _twitchApi.Webhooks.helix.StreamUpDownAsync(
                "http://destoofapi.azurewebsites.net/api/chat/channelLive", WebhookCallMode.Subscribe, userId, new TimeSpan(0,0,0, 864000));
        }

        public Task SendChatMessage(Models.Domain.Messages.ChatMessage message, string channel)
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