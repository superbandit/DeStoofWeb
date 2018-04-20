using DeStoofApi.Models;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using DeStoofApi.Chatsources.Discord;
using DeStoofApi.Chatsources.Twitch;
using DeStoofApi.EventArgs;
using DeStoofApi.Hubs;
using DeStoofApi.Models.Guilds;

namespace DeStoofApi.Services
{
    public class MessageService
    {
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly TwitchManager _twitchManager;
        private readonly DiscordManager _discordManager;
        private readonly IMongoCollection<ChatMessage> _messages;
        private readonly IMongoCollection<GuildSettings> _guildSettings;

        public MessageService(IMongoDatabase database, TwitchManager twitchManager, DiscordManager discordManager, IHubContext<ChatHub> chatHub)
        {
            var database1 = database;
            _messages = database1.GetCollection<ChatMessage>("Messages");
            _guildSettings = database1.GetCollection<GuildSettings>("guildSettings");

            _twitchManager = twitchManager;
            _twitchManager.MessageReceived += OnTwitchMessageReceived;

            _discordManager = discordManager;
            _discordManager.MessageReceived += OnDiscordMessageReceived;

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

        public async Task<bool> StartDiscordConnection()
        {
            return await _discordManager.RunBotAsync();
        }

        private async void OnTwitchMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            ChatMessage chatMessage = args.ChatMessage;

            var filter = Builders<GuildSettings>.Filter.Eq(g => g.TwitchSettings.TwitchChannel, chatMessage.Channel);
            var settings = await (await _guildSettings.FindAsync(filter)).FirstOrDefaultAsync();

            await _chatHub.Clients.All.SendAsync("Send", chatMessage.ToJson());

            if (settings.TwitchSettings.DiscordChannel != null)
                _discordManager.SendDiscordMessage((ulong) settings.TwitchSettings.DiscordChannel, chatMessage.Message);

            _messages.InsertOne(chatMessage);
        }

        private async void OnDiscordMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            ChatMessage chatMessage = args.ChatMessage;

            await _chatHub.Clients.All.SendAsync("Send", chatMessage.ToJson());

            _messages.InsertOne(chatMessage);
        }
    }
}
