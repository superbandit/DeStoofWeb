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

        [HttpPost, Route("connect/{channel}")]
        public IActionResult SendChat([FromRoute]string channel)
        {
            bool x = _MessageService.StartConnection(channel);
            if (!x)
                return BadRequest("Channel already added");

            return Ok();
        }
    }
}
