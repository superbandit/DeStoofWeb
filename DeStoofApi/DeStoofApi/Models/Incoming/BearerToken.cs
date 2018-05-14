using Microsoft.AspNetCore.Mvc;

namespace DeStoofApi.Models.Incoming
{
    public class BearerToken
    {
        [FromQuery(Name = "token_type")]
        public string TokenType { get; set; }
        [FromQuery(Name = "access_token")]
        public string AccessToken { get; set; }
    }
}
