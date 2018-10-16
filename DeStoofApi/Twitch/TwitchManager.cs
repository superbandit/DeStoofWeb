using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Core.Messages;
using Core.Settings;
using Microsoft.Extensions.Configuration;
using TwitchLib.Api;
using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Helix.Models.Users;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;

namespace Twitch
{
    public class TwitchManager : IMessagePlatform
    {
        private readonly TwitchAPI _twitchApi = new TwitchAPI();
        private readonly ITwitchClient _client;

        private readonly IConfiguration _config;

        public TwitchManager(IConfiguration config, ITwitchClient client)
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

        private void OnDisconnected(object sender, OnDisconnectedEventArgs e)
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
            return await _twitchApi.Helix.Users.GetUsersAsync(logins: channelNames);
        }

        public Task<bool> SubToChannelLiveWebhook(string userId)
        {
            return _twitchApi.Helix.Webhooks.StreamUpDownAsync(
                "http://destoofapi.azurewebsites.net/api/chat/channelLive", WebhookCallMode.Subscribe, userId,
                new TimeSpan(0, 0, 0, 864000));
        }

        public async Task StreamMessage(GuildSettings settings, IUserMessage message)
        {
            if (message is TwitchMessage) return;

            await SendMessage(settings.TwitchSettings.ChannelName, $"{message.Author}: {message.Content}");
        }

        public Task SendMessage(string sourceId, string message)
        {
            if (_client.JoinedChannels.Any(c => c.Channel == sourceId))
                _client.SendMessage(sourceId, message);

            return Task.CompletedTask;
        }
    }
}