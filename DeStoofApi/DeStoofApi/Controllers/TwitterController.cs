using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DeStoofApi.Models.Incoming;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DeStoofApi.Controllers
{
    [Route("api/twitter")]
    public class TwitterController : Controller
    {
        private readonly IConfiguration _configuration;

        public TwitterController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult ChallengeResponse([FromQuery(Name = "crc_token")] string crcToken)
        {
            var encoding = new ASCIIEncoding();
            byte[] textBytes = encoding.GetBytes(crcToken);
            byte[] keyBytes = encoding.GetBytes(_configuration["Secure:TwitterSecret"]);

            byte[] hashBytes;

            using (var hash = new HMACSHA256(keyBytes))
                hashBytes = hash.ComputeHash(textBytes);

            return Ok(new {response_token = $"sha256={Convert.ToBase64String(hashBytes)}"});
        }

        [HttpGet, Route("bearer"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Bearer()
        {
            var client = new HttpClient();
            var encoding = new ASCIIEncoding();

            var bse = Convert.ToBase64String(
                encoding.GetBytes($"{_configuration["Secure:TwitterKey"]}:{_configuration["Secure:TwitterSecret"]}"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", bse);

            var response = await client.PostAsync("https://api.twitter.com/oauth2/token",
                new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded"));

            return Ok(JsonConvert.DeserializeObject<BearerToken>(await response.Content.ReadAsStringAsync()));
        }

        [HttpGet, Route("newWebHook/{user}")]
        public async Task<IActionResult> NewWebHook([FromRoute] string user)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration["Secure:TwitterBearer"]);
            var response = await client.PostAsync($"https://api.twitter.com/1.1/account_activity/all/:{user}/webhooks.json",
                new StringContent("url=http://destoofapi.azurewebsites.net/api/twitter/hook", Encoding.UTF8, "application/x-www-form-urlencoded"));
            return Ok(JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync()));
        }

        [HttpPost, Route("hook")]
        public IActionResult OnHook()
        {
            return Ok();
        }
    }
}
