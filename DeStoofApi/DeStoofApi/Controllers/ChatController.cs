using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DeStoofApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace DeStoofApi.Controllers
{
    [Authorize]
    [Route("api/chat")]
    public class ChatController : Controller
    {
        private readonly MessageService _messageService;

        public ChatController(MessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost, Route("connectIrc/{channel}")]
        public IActionResult ConnectToIrc([FromRoute]string channel)
        {
            bool x = _messageService.StartIrcConnection(channel);
            if (!x)
                return BadRequest("Channel already added");

            return Ok();
        }

        [HttpPost, Route("startDiscord")]
        public async Task<IActionResult> ConnectToDiscord([FromRoute]string channel)
        {
            bool x = await _messageService.StartDiscordConnection();
            if (!x)
                return BadRequest("Bot already active");

            return Ok();
        }     

        [AllowAnonymous]
        [HttpGet, Route("ping")]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}
