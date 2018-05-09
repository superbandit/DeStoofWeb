using System.Threading.Tasks;
using DeStoofApi.Chatsources.Discord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeStoofApi.Controllers
{
    [Route("api/Admin"), Authorize(Roles = "Admin")]
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
            return Ok(_discordManager.GetActiveServers());
        }

        [HttpPost, Route("globalMessageToServerOwners")]
        public async Task<IActionResult> GetActiveDiscordServers([FromBody] string message)
        {
            await _discordManager.GlobalMessageToServerOwners(message);

            return Ok("Message has been sent.");
        }
    }
}
