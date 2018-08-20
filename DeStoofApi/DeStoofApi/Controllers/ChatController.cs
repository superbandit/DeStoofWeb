using System.Linq;
using System.Threading.Tasks;
using DeStoofApi.Chatsources.Discord;
using DeStoofApi.Chatsources.Twitch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Domain.Guilds;
using Models.View.External;
using Raven.Client.Documents.Session;

namespace DeStoofApi.Controllers
{
    [Route("api/chat")]
    public class ChatController : Controller
    {
        private readonly DiscordManager _discordManager;
        private readonly TwitchManager _twitchManager;
        private readonly IAsyncDocumentSession _session;

        public ChatController(DiscordManager discordManager, IAsyncDocumentSession session, TwitchManager twitchManager)
        {
            _discordManager = discordManager;
            _session = session;
            _twitchManager = twitchManager;
        }

        [HttpPost, Route("channelLive")]
        public async Task<IActionResult> ChannelLive([FromBody] StreamUpWebhook stream)
        {
            if (stream == null) return BadRequest();

            using (_session)
            {
                var settings = await _session.Query<GuildSettings>().FirstOrDefaultAsync(s => s.TwitchSettings.UserId == stream.Data.FirstOrDefault().UserId);

                if (settings.TwitchSettings.DiscordWebhookChannel != null)
                    await _discordManager.SendMessage(settings.TwitchSettings.DiscordWebhookChannel.Id, settings.TwitchSettings.WebhookMessage);

                await _twitchManager.SubToChannelLiveWebhook(settings.TwitchSettings.UserId);
            }

            return Ok();
        }

        [HttpGet, Route("ping")]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}
