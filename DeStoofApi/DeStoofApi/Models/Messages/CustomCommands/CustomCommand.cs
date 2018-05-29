using MongoDB.Bson.Serialization.Attributes;

namespace DeStoofApi.Models.Messages.CustomCommands
{
    public class CustomCommand
    {       
        [BsonConstructor]
        public CustomCommand(bool prefixAnywhere, string prefix, string inputString)
        {
            PrefixAnywhere = prefixAnywhere;
            Prefix = prefix;
            InputString = inputString;
        }

        public bool PrefixAnywhere { get; }
        public string Prefix { get; }
        public string InputString { get; }
    }
}
