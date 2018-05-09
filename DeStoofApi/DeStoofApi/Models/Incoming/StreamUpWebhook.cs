using System.Collections.Generic;
using TwitchLib.Api.Models.Helix.Streams.GetStreams;

namespace DeStoofApi.Models.Incoming
{
    public class StreamUpWebhook
    {
        public List<Stream> Data { get; set; }
    }
}
