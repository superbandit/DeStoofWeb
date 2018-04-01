using DeStoofApi.Chatsources;
using DeStoofApi.EventArguments;
using DeStoofApi.Models;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DeStoofApi.Services
{
    public class MessageService
    {
        IHubContext<ChatHub> ChatHub;
        IrcManager IrcManager;
        IMongoDatabase Database;
        IMongoCollection<BsonDocument> Messages;

        public MessageService(IMongoDatabase database, IrcManager ircManager, IHubContext<ChatHub> chatHub)
        {
            Database = database;
            Messages = Database.GetCollection<BsonDocument>("Messages");

            IrcManager = ircManager;
            IrcManager.MessageReceived += OnIrcMessageReceived;

            ChatHub = chatHub;
        }

        public bool StartConnection(string channel)
        {
            return IrcManager.StartConnection(channel);
        }

        private async void OnIrcMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            ChatMessage chatMessage = args.ChatMessage;

            await ChatHub.Clients.All.SendAsync("Send", chatMessage.ToJson());

            BsonDocument chatMessageBson = chatMessage.ToBsonDocument();
            Messages.InsertOne(chatMessageBson);
        }
    }
}
