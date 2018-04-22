using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeStoofApi.Models.ChatMessages
{
    [BsonDiscriminator(RootClass = true)]
    [BsonKnownTypes(typeof(DiscordChatMessage), typeof(TwitchChatMessage))]
    public abstract class ChatMessage
    {
        public ObjectId Id { get; set; }
        public string User { get; set; }
        public string Message { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Date { get; set; }
    }
}
