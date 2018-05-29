using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeStoofApi.Models.Messages
{
    [BsonDiscriminator(RootClass = true)]
    [BsonKnownTypes(typeof(DiscordChatMessage), typeof(TwitchChatMessage))]
    public abstract class ChatMessage
    {
        public ObjectId Id { get; set; }
        public ulong GuildId { get; set; }
        public string User { get; set; }
        public string Message { get; set; }
        public Enums.ChatPlatforms SendTo { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Date { get; set; }
    }
}
