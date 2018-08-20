using Newtonsoft.Json;

namespace Models.View.External
{
    public class BearerToken
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
