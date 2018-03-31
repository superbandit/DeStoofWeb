using DeStoofApi.Chatsources;
using DeStoofApi.EventArguments;
using DeStoofApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DeStoofApi.Services
{
    public class MessageService
    {
        ChatHub ChatHub;
        IrcManager IrcManager;
        IMongoDatabase Database;
        IMongoCollection<BsonDocument> Messages;

        public MessageService(IMongoDatabase database, IrcManager ircManager)
        {
            Database = database;
            Messages = Database.GetCollection<BsonDocument>("Messages");

            IrcManager = ircManager;
            IrcManager.MessageReceived += OnIrcMessageReceived;

            ChatHub = new ChatHub();
        }

        private async void OnIrcMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            ChatMessage chatMessage = args.ChatMessage;

            BsonDocument chatMessageBson = chatMessage.ToBsonDocument();
            Messages.InsertOne(chatMessageBson);

            await ChatHub.Send(chatMessage);
        }
    }
}
