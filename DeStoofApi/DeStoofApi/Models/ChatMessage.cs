using MongoDB.Bson;
using static DeStoofApi.Models.Enums;

namespace DeStoofApi.Models
{
    public class ChatMessage
    {
        public ObjectId Id { get; set; }
        public Platforms Platform { get; set; }
        public string User { get; set; }
        public string Channel { get; set; }
        public string Message { get; set; }
        public string Date { get; set; }
    }
}
