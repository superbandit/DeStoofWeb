using System;
using System.Linq;
using System.Threading.Tasks;
using DeStoofApi.Chatsources.Discord;
using DeStoofApi.Models.Guilds;
using DeStoofApi.Models.Incoming;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace DeStoofApi.Controllers
{
    [Route("api/chat")]
    public class ChatController : Controller
    {
        private readonly DiscordManager _discordManager;
        private readonly IMongoCollection<GuildSettings> _guildSettings;

        public ChatController(DiscordManager discordManager, IConfiguration config, IMongoDatabase database)
        {
            _discordManager = discordManager;
            _guildSettings = database.GetCollection<GuildSettings>(config["Secure:GuildSettings"]);
        }

        [HttpPost, Route("channelLive")]
        public async Task<IActionResult> ChannelLive([FromBody] StreamUpWebhook stream)
        {
            var settings = await (await _guildSettings.FindAsync(s => s.TwitchSettings.UserId == Int32.Parse(stream.Data.FirstOrDefault().UserId))).FirstOrDefaultAsync();

            if (stream == null) return Ok();

            if (settings.TwitchSettings.WebhookDiscordChannel != null)
                _discordManager.SendMessage((ulong) settings.TwitchSettings.WebhookDiscordChannel,
                    settings.TwitchSettings.WebhookMessage);

            return Ok();
        }

        [HttpGet, Route("ping")]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}
