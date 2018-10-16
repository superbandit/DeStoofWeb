using System.Linq;
using System.Threading.Tasks;
using Core.Settings;
using DeStoofApi.View.External;
using Discord;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Twitch;

namespace DeStoofApi.Controllers
{
    [Route("api/chat")]
    public class ChatController : Controller
    {
        private readonly DiscordManager _discordManager;
        private readonly TwitchManager _twitchManager;
        private readonly IDocumentStore _documentStore;

        public ChatController(DiscordManager discordManager, TwitchManager twitchManager, IDocumentStore documentStore)
        {
            _discordManager = discordManager;
            _twitchManager = twitchManager;
            _documentStore = documentStore;
        }

        [HttpPost, Route("channelLive")]
        public async Task<IActionResult> ChannelLive([FromBody] StreamUpWebhook stream)
        {
            if (stream == null) return BadRequest();

            using (var session = _documentStore.OpenAsyncSession())
            {
                var settings = await session.Query<GuildSettings>().FirstOrDefaultAsync(s => s.TwitchSettings.UserId == stream.Data.FirstOrDefault().UserId);

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
