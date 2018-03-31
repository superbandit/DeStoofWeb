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
        readonly ChatHub _hubService;
        readonly IrcManager _ircManager;

        public ChatController(ChatHub hubservice, IrcManager ircManager)
        {
            _hubService = hubservice;
            _ircManager = ircManager;
        }

        [HttpPost, Route("send")]
        public async Task<IActionResult> SendChat(ChatMessage message)
        {
            await _hubService.Send(message);

            return Ok();
        }
    }
}
