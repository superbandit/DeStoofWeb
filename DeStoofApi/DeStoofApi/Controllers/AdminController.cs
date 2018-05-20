using System;
using System.Threading.Tasks;
using DeStoofApi.Chatsources.Discord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeStoofApi.Controllers
{
    [Route("api/admin"), Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly DiscordManager _discordManager;

        public AdminController(DiscordManager discordManager)
        {
            _discordManager = discordManager;
        }

        [HttpGet, Route("getActiveDiscordServers")]
        public IActionResult GetActiveDiscordServers()
        {
            try
            {
                return Ok(_discordManager.GetActiveServers());
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost, Route("globalMessageToServerOwners")]
        public async Task<IActionResult> GetActiveDiscordServers([FromBody] string message)
        {
            try
            {
                await _discordManager.GlobalMessageToServerOwners(message);

                return Ok("Message has been sent.");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost, Route("partGuild/{guildId}")]
        public async Task<IActionResult> PartGuild([FromRoute] ulong guildId)
        {
            try
            {
                await _discordManager.PartGuild(guildId);

                return Ok("Left the guild.");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

    }
}
