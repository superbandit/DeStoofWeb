using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using DeStoofApi.Services;
using DeStoofApi.Chatsources;
using DeStoofApi.Models;

namespace DeStoofApi.Controllers
{
    [Route("api/chat")]
    public class ChatController : Controller
    {
        readonly MessageService _MessageService;

        public ChatController(MessageService messageService)
        {
            _MessageService = messageService;
        }

        [HttpPost, Route("connectIrc/{channel}")]
        public IActionResult ConnectToIrc([FromRoute]string channel)
        {
            bool x = _MessageService.StartIrcConnection(channel);
            if (!x)
                return BadRequest("Channel already added");

            return Ok();
        }

        [HttpPost, Route("connectDiscord/{channel}")]
        public IActionResult ConnectToDiscord([FromRoute]string channel)
        {
            _MessageService.StartDiscordConnection();

            return Ok();
        }
    }
}
