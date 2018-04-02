using DeStoofApi.Chatsources;
using DeStoofApi.EventArguments;
using DeStoofApi.Models;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace DeStoofApi.Services
{
    public class MessageService
    {
        IHubContext<ChatHub> ChatHub;
        IrcManager IrcManager;
        DiscordManager DiscordManager;
        IMongoDatabase Database;
        IMongoCollection<BsonDocument> Messages;

        public MessageService(IMongoDatabase database, IrcManager ircManager, DiscordManager discordManager, IHubContext<ChatHub> chatHub)
        {
            Database = database;
            Messages = Database.GetCollection<BsonDocument>("Messages");

            IrcManager = ircManager;
            IrcManager.MessageReceived += OnIrcMessageReceived;

            DiscordManager = discordManager;
            DiscordManager.MessageReceived += OnDiscordMessageReceived;

            ChatHub = chatHub;
        }

        public bool StartIrcConnection(string channel)
        {
            return IrcManager.StartConnection(channel);
        }

        public async Task StartDiscordConnection()
        {
            await DiscordManager.RunBotAsync();
            //TODO what channel?
        }

        private async void OnIrcMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            ChatMessage chatMessage = args.ChatMessage;

            await ChatHub.Clients.All.SendAsync("Send", chatMessage.ToJson());

            BsonDocument chatMessageBson = chatMessage.ToBsonDocument();
            Messages.InsertOne(chatMessageBson);
        }

        private async void OnDiscordMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            ChatMessage chatMessage = args.ChatMessage;

            await ChatHub.Clients.All.SendAsync("Send", chatMessage.ToJson());

            BsonDocument chatMessageBson = chatMessage.ToBsonDocument();
            Messages.InsertOne(chatMessageBson);
        }
    }
}
