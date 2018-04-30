using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System.Threading.Tasks;
using DeStoofApi.Chatsources.Discord;
using DeStoofApi.Chatsources.Twitch;
using DeStoofApi.EventArgs;
using DeStoofApi.Hubs;
using DeStoofApi.Models;
using DeStoofApi.Models.ChatMessages;
using DeStoofApi.Models.Guilds;
using Microsoft.Extensions.Configuration;

namespace DeStoofApi.Services
{
    public class MessageService
    {
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly TwitchManager _twitchManager;
        private readonly DiscordManager _discordManager;
        private readonly IMongoCollection<ChatMessage> _messages;
        private readonly IMongoCollection<GuildSettings> _guildSettings;

        public MessageService(IMongoDatabase database, TwitchManager twitchManager, DiscordManager discordManager, IHubContext<ChatHub> chatHub, IConfiguration config)
        {
            var database1 = database;
            _messages = database1.GetCollection<ChatMessage>(config["Secure:Messages"]);
            _guildSettings = database1.GetCollection<GuildSettings>(config["Secure:GuildSettings"]);

            _twitchManager = twitchManager;
            _twitchManager.MessageReceived += OnChatMessageReceived;

            _discordManager = discordManager;
            _discordManager.MessageReceived += OnChatMessageReceived;

            _chatHub = chatHub;
        }

        public void StartTwitchConnection()
        {
            _twitchManager.Start();
        }

        public bool JoinTwitchChannel(string channel)
        {
            return _twitchManager.JoinTwitchChannel(channel);
        }

        public bool LeaveTwitchChannel(string channel)
        {
            return _twitchManager.LeaveTwitchChannel(channel);
        }


        public async Task<bool> StartDiscordConnection()
        {
            return await _discordManager.RunBotAsync();
        }

        private async Task OnChatMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            await SendToPlatforms(args.ChatMessage);

            await _chatHub.Clients.All.SendAsync("Send", args.ChatMessage);

            _messages.InsertOne(args.ChatMessage);
        }

        private async Task<GuildSettings> GetGuildSettings(ulong guildId)
        {
            var filter = Builders<GuildSettings>.Filter.Eq(g => g.GuildId, guildId);
            return await (await _guildSettings.FindAsync(filter)).FirstOrDefaultAsync();
        }

        private async Task SendToPlatforms(ChatMessage message)
        {
            foreach (var guildId in message.GuildIds)
            {
                var settings = await GetGuildSettings(guildId);

                if (message is DiscordChatMessage a && a.ChannelId != settings.TwitchSettings.DiscordChannel) return;

                if (message.Message.StartsWith(settings.CommandPrefix))
                    return;

                if (message.SendTo.HasFlag(Enums.ChatPlatforms.Discord))
                    if (settings.TwitchSettings.DiscordChannel != null)
                        _discordManager.SendDiscordMessage((ulong) settings.TwitchSettings.DiscordChannel, message);
                if (message.SendTo.HasFlag(Enums.ChatPlatforms.Twitch))
                    await _twitchManager.SendMessage(message, settings.TwitchSettings.TwitchChannel);
            }

        }
    }
}
